using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace GlobeAuction.Steps
{
    public class CommonSteps
    {
        [AfterScenario("hook_purgeall")]
        public void AfterScenarioPurgeAll()
        {

        }
    }
}
