// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.



namespace TechBox.STS.Controllers.Device
{
    using TechBox.STS.Controllers.Consent;
    public class DeviceAuthorizationInputModel : ConsentInputModel
    {
        public string UserCode { get; set; }
    }
}