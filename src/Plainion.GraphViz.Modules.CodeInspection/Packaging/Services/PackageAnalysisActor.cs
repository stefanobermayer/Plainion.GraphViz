﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Newtonsoft.Json;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    class PackageAnalysisActor : ReceiveActor, IWithUnboundedStash
    {
        private CancellationTokenSource myCTS;

        public IStash Stash { get; set; }

        public PackageAnalysisActor()
        {
            myCTS = new CancellationTokenSource();

            Ready();
        }

        private void Ready()
        {
            Receive<AnalysisRequest>( r =>
            {
                Console.WriteLine( "WORKING" );

                var self = Self;
                var sender = Sender;

                Task.Run<AnalysisDocument>( () =>
                {
                    var analyzer = new PackageAnalyzer();

                    if( r.AnalysisMode == AnalysisMode.InnerPackageDependencies )
                    {
                        analyzer.PackagesToAnalyze.Add( r.PackageName );
                    }

                    var spec = SpecUtils.Deserialize( SpecUtils.Unzip( r.Spec ) );
                    return analyzer.Execute( spec, myCTS.Token );
                }, myCTS.Token )
                .ContinueWith<object>( x =>
                {
                    if( x.IsCanceled || x.IsFaulted )
                    {
                        // https://github.com/akkadotnet/akka.net/issues/1409
                        // -> exceptions are currently not serializable in raw version
                        //return x.Exception;
                        return new Finished { Error = x.Exception.ToString() };
                    }

                    Console.WriteLine( "Writing response ..." );

                    var serializer = new AnalysisDocumentSerializer();
                    serializer.Serialize( x.Result, r.OutputFile );

                    return new Finished { ResponseFile = r.OutputFile };
                }, TaskContinuationOptions.ExecuteSynchronously )
                .PipeTo( self, sender );

                Become( Working );
            } );
        }

        private void Working()
        {
            Receive<Cancel>( msg =>
            {
                Console.WriteLine( "CANCELED" );

                myCTS.Cancel();

                Sender.Tell( "canceled" );

                BecomeReady();
            } );
            Receive<Finished>( msg =>
            {
                if( msg.Error != null )
                {
                    // https://github.com/akkadotnet/akka.net/issues/1409
                    // -> exceptions are currently not serializable in raw version
                    Sender.Tell( new FailureResponse { Error = msg.Error } );
                }
                else
                {
                    Sender.Tell( msg.ResponseFile );
                }

                Console.WriteLine( "FINISHED" );

                BecomeReady();
            } );
            ReceiveAny( o => Stash.Stash() );
        }

        private void BecomeReady()
        {
            myCTS = new CancellationTokenSource();
            Stash.UnstashAll();
            Become( Ready );
        }
    }
}
