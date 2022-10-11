// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.




namespace TechBox.STS.Controllers.Device
{
    using TechBox.STS.Controllers.Consent;
    public class DeviceAuthorizationViewModel : ConsentViewModel
    {
        public string UserCode { get; set; }
        public bool ConfirmUserCode { get; set; }
    }
}