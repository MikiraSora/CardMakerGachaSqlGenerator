using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardMakerGachaSqlGenerator.Base
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SqlColnumAttribute : Attribute
    {
        public SqlColnumAttribute(string sqlColnumName)
        {
            SqlColnumName = sqlColnumName;
        }

        public string SqlColnumName { get; }
    }
}
