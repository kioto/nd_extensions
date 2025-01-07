using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextDesign.Core;

namespace LoadUCScenario
{
    internal class NDTools
    {
        static public IModel findPackage(IModel parent, string pathName)
        {
            IModel package = null;
            foreach (var model in parent.FindChildrenByClassDisplayName("パッケージ", recursive: true))
            {
                if (model.ModelPath == pathName)
                {
                    package = model;
                    break;
                }
            }

            return package;
        }

        static public IModel findActor(IModel parent, string actorName)
        {
            IModel actor = null;
            foreach(var model in  parent.FindChildrenByClassDisplayName("アクター", recursive: true))
            {
                if (model.Name == actorName)
                {
                    actor = model;
                }
            }

            return actor;
        }
    }
}
