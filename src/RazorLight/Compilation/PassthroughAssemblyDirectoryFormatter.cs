using System;
using System.IO;

namespace RazorLight.Compilation
{
  public class PassthroughAssemblyDirectoryFormatter : IAssemblyDirectoryFormatter
  {
    public string GetAssemblyDirectory(Assembly assembly)
    {
      return Path.GetDirectoryName(assembly.Location);
    }
  }
}
