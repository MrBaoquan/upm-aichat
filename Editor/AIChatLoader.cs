using System.Collections;
using System.Collections.Generic;
using UNIHper.Editor;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class AIChatLoader
{
    static AIChatLoader()
    {
        AssemblyCfgUtil.AddAssembly("AIChat.Runtime");
        LinkerCfgUtil.PreserveAssembly("DeepSeek.Sdk");
    }
}
