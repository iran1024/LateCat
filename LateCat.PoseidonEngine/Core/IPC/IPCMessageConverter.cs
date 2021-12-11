using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace LateCat.PoseidonEngine.Core
{
    class IPCMessageConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPCMessage);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            return (MessageType)jo["Type"]!.Value<int>() switch
            {
                MessageType.cmd_reload => jo.ToObject<IPCReloadCmd>(serializer),
                MessageType.cmd_close => jo.ToObject<IPCCloseCmd>(serializer),
                MessageType.cmd_screenshot => jo.ToObject<IPCScreenshotCmd>(serializer),
                MessageType.lsp_perfcntr => jo.ToObject<IPCSystemInformation>(serializer),
                MessageType.lp_slider => jo.ToObject<IPCSlider>(serializer),
                MessageType.lp_textbox => jo.ToObject<IPCTextBox>(serializer),
                MessageType.lp_dropdown => jo.ToObject<IPCDropdown>(serializer),
                MessageType.lp_fdropdown => jo.ToObject<IPCFolderDropdown>(serializer),
                MessageType.lp_button => jo.ToObject<IPCButton>(serializer),
                MessageType.lp_cpicker => jo.ToObject<IPCColorPicker>(serializer),
                MessageType.lp_chekbox => jo.ToObject<IPCCheckbox>(serializer),
                MessageType.msg_console => jo.ToObject<IPCMessageConsole>(serializer),
                MessageType.msg_hwnd => jo.ToObject<IPCMessageHwnd>(serializer),
                MessageType.msg_screenshot => jo.ToObject<IPCMessageScreenshot>(serializer),
                MessageType.msg_wploaded => jo.ToObject<IPCMessageWallpaperLoaded>(serializer),
                _ => null,
            };
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
