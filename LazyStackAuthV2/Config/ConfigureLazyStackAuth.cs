using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LazyStack.Utils;

namespace LazyStackAuthV2
{
    public static class ConfigureLazyStackAuth
    {
        public static IMessages AddlazyStackAuth(this IMessages messages)
        {
            var assembly = MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly;
            var assemblyName = assembly!.GetName().Name;

            using var messagesStream = assembly.GetManifestResourceStream($"{assemblyName}.Config.Messages.json")!;
            // Add/Overwrite messages with messages in this library's Messages.json
            if (messagesStream != null)
            {
                using var messagesReader = new StreamReader(messagesStream);
                var messagesText = messagesReader.ReadToEnd();
                messages.MergeJson(messagesText);
            }
            return messages;
        }
    }
}
