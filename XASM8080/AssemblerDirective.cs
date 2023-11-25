using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;
internal class AssemblerDirective {
    /**
     * **FUTURE**
     * Directives generally produce no code, but vary the operation of the assembler.  Many are settings.
     * #PRAGMA TITLE NAME_OF_PROGRAM
     * #PRAGMA LINEWIDTH 80|132|nnnn
     * #PRAGMA PAGEHEIGHT 60|nnnn
     * #PRAGMA LABEL UPPER|MIXED|LOWER MATCH|NOMATCH
     * #PRAGMA INSTRUCTION UPPER|MIXED|LOWER
     * #PRAGMA OVERFLOW IGNORE|WARN|ERROR
     * #PRAGMA BYTEOVERFLOW IGNORE|WARN|ERROR
     * #PRAGMA WORDOVERFLOW IGNORE|WARN|ERROR
     * #PRAGMA ERRORLIMIT 10|nnnn
     **/
}
