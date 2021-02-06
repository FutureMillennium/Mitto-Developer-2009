using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace MittoDeveloper
{
  class MittoClass
  {
    public string Name;
    public List<string> Variables;
    public ArrayList Values;

    public MittoClass()
    {
      Variables = new List<string>();
      Values = new ArrayList();
    }

    public MittoClass(string strName)
    {
      Name = strName;
      Variables = new List<string>();
      Values = new ArrayList();
    }

    public MittoClass(MittoClass mtcCopyFrom)
    {
      Name = mtcCopyFrom.Name;
      Variables = new List<string>(mtcCopyFrom.Variables);
      Values = new ArrayList(mtcCopyFrom.Values);
    }
  }
}
