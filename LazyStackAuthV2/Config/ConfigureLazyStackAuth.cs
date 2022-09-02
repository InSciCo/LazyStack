using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyStack.Utils;

namespace LazyStackAuthV2
{
    public static class ConfigureLazyStackAuth
    {
        public static IMessages AddlazyStackAuth(this IMessages messages)
        {
            using var messagesStream = typeof(ConfigureLazyStackAuth).Assembly.GetManifestResourceStream("LazyStackAuthV2.Config.Messages.json")!;
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
