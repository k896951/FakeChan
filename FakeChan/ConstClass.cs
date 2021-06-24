using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeChan
{
    static class ConstClass
    {
        public class ComboBoxListDataInterface
        {
            public string LabelData { get; set; }
            public ListenInterface ValueData { get; set; }

            public ComboBoxListDataInterface(string label, ListenInterface key)
            {
                LabelData = label;
                ValueData = key;
            }
        }

        public class ComboBoxListDataCallMethod
        {
            public string LabelData { get; set; }
            public Methods ValueData { get; set; }

            public ComboBoxListDataCallMethod(string label, Methods key)
            {
                LabelData = label;
                ValueData = key;
            }
        }

        public class ComboListDataBouyomiVoice
        {
            public string LabelData { get; set; }
            public BouyomiVoice ValueData { get; set; }

            public ComboListDataBouyomiVoice(string label, BouyomiVoice key)
            {
                LabelData = label;
                ValueData = key;
            }
        }

        public static List<ComboBoxListDataInterface> BouyomiInterface = new List<ComboBoxListDataInterface>()
        {
            new ComboBoxListDataInterface("IPC : BouyomiChan",   ListenInterface.IPC1     ),
            new ComboBoxListDataInterface("Socket : 50001",      ListenInterface.Socket1  ),
            new ComboBoxListDataInterface("Socket : 50002",      ListenInterface.Socket2  ),
            new ComboBoxListDataInterface("HTTP : 50080",        ListenInterface.Http1    ),
            new ComboBoxListDataInterface("HTTP : 50081",        ListenInterface.Http2    ),
            new ComboBoxListDataInterface("Clipboard",           ListenInterface.Clipboard )
        };

        public static List<ComboBoxListDataCallMethod> BouyomiCallMethod = new List<ComboBoxListDataCallMethod>()
        {
            new ComboBoxListDataCallMethod("同期（共通キューに入れて順次発声）",   Methods.sync    ),
            new ComboBoxListDataCallMethod("非同期（すぐに発声）",                 Methods.async   )
        };

        public static List<ComboListDataBouyomiVoice> BouyomiVoiceName = new List<ComboListDataBouyomiVoice>()
        {
            new ComboListDataBouyomiVoice("ボイス0",   BouyomiVoice.voice0    ),
            new ComboListDataBouyomiVoice("女性1",     BouyomiVoice.female1   ),
            new ComboListDataBouyomiVoice("女性2",     BouyomiVoice.female2   ),
            new ComboListDataBouyomiVoice("男性1",     BouyomiVoice.male1     ),
            new ComboListDataBouyomiVoice("男性2",     BouyomiVoice.male2     ),
            new ComboListDataBouyomiVoice("中性",      BouyomiVoice.nogender  ),
            new ComboListDataBouyomiVoice("ロボット",  BouyomiVoice.robot     ),
            new ComboListDataBouyomiVoice("機械1",     BouyomiVoice.voice0    ),
            new ComboListDataBouyomiVoice("機械2",     BouyomiVoice.voice0    ),
        };

        public static Dictionary<int, string> RandomAssignMethod = new Dictionary<int, string>()
        {
            { 0, "ランダム適用しない" },
            { 1, "ボイス0～機械2をランダムに適用する" },
            { 2, "音声合成製品話者をランダムに適用する" }
        };

        public static Dictionary<int, string> BouyomiVoiceMap = new Dictionary<int, string>()
        {
            { 0, "ボイス0"  },
            { 1, "女性1"    },
            { 2, "女性1"    },
            { 3, "男性1"    },
            { 4, "男性2"    },
            { 5, "中性"     },
            { 6, "ロボット" },
            { 7, "機械1"    },
            { 8, "機械2"    }
        };

        public static Dictionary<int, string> ListenInterfaceMap = new Dictionary<int, string>()
        {
            { 0, "IPC : BouyomiChan" },
            { 1, "Socket : 50001"    },
            { 2, "Http : 50080"      },
            { 3, "Socket2 : 50002"   },
            { 4, "Http2 : 50081"     },
            { 5, "Clipboard"         }
        };

    }
}
