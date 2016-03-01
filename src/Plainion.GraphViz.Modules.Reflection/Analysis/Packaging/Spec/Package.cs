﻿using System.Collections.Generic;
using System.Windows.Markup;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Spec
{
    [ContentProperty("Patterns")]
    public class Package : PackageBase
    {
        public Package()
        {
            Clusters = new List<Cluster>();
        }

        public List<Cluster> Clusters { get; private set; }
    }
}
