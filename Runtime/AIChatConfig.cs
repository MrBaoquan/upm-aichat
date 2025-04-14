using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UNIHper;
using System.Xml.Serialization;
using DNHper;

namespace AIChat
{
    [UnityEngine.Scripting.Preserve]
    [SerializedAt(AppPath.StreamingDir)]
    public class AIChatConfig : UConfig
    {
        [XmlAttribute]
        public string AppKey = string.Empty;

        [XmlIgnore]
        public string DecryptedAppKey = string.Empty;

        // Write your comments here
        protected override string Comment()
        {
            return @"
        Write your comments here...
        ";
        }

        // Called once after the config data is loaded
        protected override void OnLoaded()
        {
            if (string.IsNullOrEmpty(AppKey))
            {
                AppKey = AES.Encrypt("sk-1ae18b8ce5fd4392b13e5a22ff41219a", "mrbaoquan");
                this.Save();
            }

            DecryptedAppKey = AES.Decrypt(AppKey, "mrbaoquan");
        }
    }
}
