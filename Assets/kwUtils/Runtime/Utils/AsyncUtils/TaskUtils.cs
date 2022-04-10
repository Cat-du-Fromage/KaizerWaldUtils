using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KWUtils
{
    public static class TaskUtils
    {
        public static CancellationToken RefreshToken(ref CancellationTokenSource tokenSource) {
            tokenSource?.Cancel();
            tokenSource?.Dispose();
            tokenSource = new CancellationTokenSource();
            return tokenSource.Token;
        }
    }
}
