using MatthiWare.CommandLine.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Options
{
    public class ProgramOptions
    {
        [Name("s", "pagesize"), Description("page size in mb"), DefaultValue(1000)]
        public long PageSize { get; set; }
        [Required, Name("f", "filename"), Description("source file name") ]
        public string FileName { get; set; }
        [Name("o", "outputfile"), Description("output file name"), DefaultValue("")]
        public string OutputFile { get; set; }
        [Name("g", "generatekey"), Description("generate new key"), DefaultValue(true)]
        public bool GenerateKey { get; set; }
        [Name("c", "constkey"), Description("device key"), DefaultValue("")]
        public string DeviceKey { get; set; }

        [Name("d", "decrypt"), Description("need decrypt file"), DefaultValue(false)]
        public bool NeedDecryptFile { get; set; }

        [Name("a", "array"), Description("array")]
        public int[] arr { get; set; }

        [Name("t", "decrypt"), Description("array of datetime")]
        public DateTime[] dateTime { get; set; }
    }
}
