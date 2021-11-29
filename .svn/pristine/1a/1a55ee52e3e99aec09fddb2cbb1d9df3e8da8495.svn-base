using System.Xml;

namespace LateCat
{
    class NotificationService
    {
        //todo: require msix package
        //https://docs.microsoft.com/en-us/windows/apps/desktop/modernize/modernize-wpf-tutorial-5
        public void ShowNotification(string description, double amount)
        {
            string xml = $@"<toast>
                      <visual>
                        <binding template='ToastGeneric'>
                          <text>Expense added</text>
                          <text>Description: {description} - Amount: {amount} </text>
                        </binding>
                      </visual>
                    </toast>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);            
        }
    }
}
