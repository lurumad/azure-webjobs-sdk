﻿using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Jobs
{
    internal interface ITriggerMap
    {
        // Scope can be a user's site. 
        Trigger[] GetTriggers(string scope);

        void AddTriggers(string scope, params Trigger[] triggers);

        IEnumerable<string> GetScopes();

        void ClearTriggers(string scope);
    }
}
