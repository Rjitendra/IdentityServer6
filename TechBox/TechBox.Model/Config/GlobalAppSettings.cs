

namespace TechBox.Model.Config
{
    public class GlobalAppSettings
    {
        public GlobalAppSettings()
        {
        }

        /// <summary>
        /// This represents how many minutes each timeslot should have in the reservation system.
        /// The application is design to have this on of three values: 15, 30, or 60.
        /// </summary>
        public int DefaultMinutesPerTimeSlot { get; set; }

        /// <summary>
        /// Controls the API to only return four slots per call
        /// </summary>
        public int SlotsToDisplayPerStore { get; set; }

        /// <summary>
        /// this represents the defualt time to put selected slot in held state, so user wont loose the selected slot until they confirm the reservation.
        /// </summary>
        public int DefaultMinutesForHeldState { get; set; }
    }
}
