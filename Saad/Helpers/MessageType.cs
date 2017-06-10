using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Saad.Helpers {
    public class MessageType {

        #region Properties

        public string Name { private set; get; }
        public string Css { private set; get; }

        public string ToastrNotification { private set; get; }

        #endregion

        private MessageType(string name, string css, string toastr) {
            Name = name;
            Css = css;
            ToastrNotification = toastr;
        }

        #region Types

        public static MessageType[] Types = new MessageType[] { Danger, Info, Success, Warning };

        public static MessageType Danger {
            get {
                return new MessageType("Danger", "alert-danger", "error");
            }
        }

        public static MessageType Info {
            get {
                return new MessageType("Info", "alert-info", "info");
            }
        }

        public static MessageType Success {
            get {
                return new MessageType("Success", "alert-success", "success");
            }
        }

        public static MessageType Warning {
            get {
                return new MessageType("Warning", "alert-warning", "warning");
            }
        }

        #endregion

        #region Methods

        public static MessageType GetByKey(string key) {
            if (key == MessageType.Danger.Name) return Danger;
            if (key == MessageType.Info.Name) return Info;
            if (key == MessageType.Warning.Name) return Warning;
            if (key == MessageType.Success.Name) return Success;
            throw new ApplicationException("Message Type not found");
        }

        #endregion

    }
}