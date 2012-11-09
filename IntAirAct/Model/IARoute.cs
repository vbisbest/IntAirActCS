using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntAirAct
{
    public class IARoute
    {
        public String Action { get; private set; }
        public String Resource { get; private set; }

        public IARoute(string action, string resource)
        {
            this.Action = action;
            this.Resource = resource;
        }

        public override string ToString()
        {
            return String.Format("IARoute[Action: {0}, Resource: {1}]", Action, Resource);
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            IARoute p = obj as IARoute;
            if ((System.Object)p == null)
            {
                return false;
            }

            if (this.Action.Equals(p.Action) && this.Resource.Equals(p.Resource)) {
                return true;
            }

            return false;
        }

        public override int GetHashCode () {
            return Action.GetHashCode() ^ Resource.GetHashCode();
        }
    }
}
