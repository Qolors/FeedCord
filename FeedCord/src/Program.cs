using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedCord.src
{
    public static class Program
    {
        private static void Main(string[] args) =>
            new Startup().Initiliaze(args).GetAwaiter().GetResult();
    }
}
