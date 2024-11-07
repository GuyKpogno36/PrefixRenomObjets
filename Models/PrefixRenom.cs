using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrefixRenomObjets.Models
{
    public class PrefixRenom
    {
        public string PREFIX_TERM { get; set; }
        public string TYPE_FICHIER { get; set; }
        public IEnumerable<TYPE_FICHIERS> listTypesFichiers { get; set; }
    }

    public class TYPE_FICHIERS
    {
        public string TYPE_FICHIER { get; set; }
        public string LIB_TYPE_FICHIER { get; set; }
    }
}