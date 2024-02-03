using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace quest_bootloader_unlocker
{
    public class Patches
    {

        public List<PatchesVersion> Versions { get; private set; }

        public Patches()
        {
            Versions =
            [
                new PatchesVersion("q2_9248600200800000.pe", new Dictionary<int, byte>() {
                        { 0x3e4c8, 0x21 },

                        { 0x3e4dc, 0x08 },
                        { 0x3e4dc + 1, 0x00 },
                        { 0x3e4dc + 2, 0x00 },
                        { 0x3e4dc + 3, 0x14 },

                        { 0x3e4fc + 0, 0x28 },
                        { 0x3e4fc + 1, 0x00 },
                        { 0x3e4fc + 2, 0x80 },
                        { 0x3e4fc + 3, 0xd2 },

                        { 0x3e534 + 0, 0x20 },
                        { 0x3e534 + 1, 0x00 },
                        { 0x3e534 + 2, 0x80 },
                        { 0x3e534 + 3, 0xd2 },
                    }),
            ];
        }


        public class PatchesVersion
        {
            public string Name { get; private set; }
            public string File { get; private set; }

            public Dictionary<int, byte> Patches { get; private set; }
            public int MaxAddress { get => Patches.Keys.Max(); }

            public PatchesVersion(string file, IEnumerable<KeyValuePair<int, byte>> patches) : this(file, file, patches) { }

            public PatchesVersion(string name, string file, IEnumerable<KeyValuePair<int, byte>> patches)
            {
                Name = name;
                File = file;
                Patches = new Dictionary<int, byte>(patches);
            }


            public bool ApplyTo(ref byte[] buffer)
            {
                if (buffer.Length <= MaxAddress)
                    return false;
                foreach (var patch in Patches)
                {
                    buffer[patch.Key] = patch.Value;
                }
                return true;
            }

        }
    }
}
