// LcpMvc.Lcp.Paychex.Supervisor.cs
// Craig Venz - 11/21/2019 - 1:48 PM

namespace Paychex.Api.Models.Workers {
    /// <summary>
    /// Worker's supervisor
    /// </summary>
    public class Supervisor
    {
        /// <summary>
        /// The unique identifier associated with this worker's supervisor. 
        /// </summary>
        public string WorkerId { get; set; }
        /// <summary>
        /// Information about the workers name. See <see cref="Workers.Name">Name</see>.
        /// </summary>
        public Name Name { get; set; }
        /// <summary>
        /// debug info
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"Supervisor: {Name}";
    }
}