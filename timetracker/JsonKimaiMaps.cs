using System;
namespace JsonKimaiMaps
{
    /// <summary>
    /// Authenticate.
    /// </summary>
    public class AuthenticateMap
    {
        public string id { get; set; }
        public string jsonrpc { get; set; }
        public AuthenticateResult Result;
    }

    public class AuthenticateResult
    {
        public bool Success { get; set; }
        public string Total { get; set; }
        public ApiKey[] Items;
    }

    public class ApiKey
    {
        // Authenticate
        public string apiKey { get; set; }

    }

    /// <summary>
    /// Get active recording.
    /// </summary>
    public class ActiveRecordingMap
    {
        public string id { get; set; }
        public string jsonrpc { get; set; }
        public ActiveRecordingResult Result;
    }

    public class ActiveRecordingResult
    {
        public bool Success { get; set; }
        public string Total { get; set; }
        public ActiveRecording[] Items;
    }

    public class ActiveRecording
    {
        // Active Recording
        public int timeEntryID { get; set; }
        public int activityID { get; set; }
        public int projectID { get; set; }
        public UInt32 start { get; set; }
        public UInt32 end { get; set; }
        public UInt32 duration { get; set; }
        public UInt32 servertime { get; set; }
        public int customerID { get; set; }
        public string customerName { get; set; }
        public string projectName { get; set; }
        public string activityName { get; set; }
    }


    /*
    /// <summary>
    /// Get projects
    /// </summary>
    public class ProjectMap
    {
        public string id { get; set; }
        public string jsonrpc { get; set; }
        public ProjectResult Result;
    }

    public class ProjectResult
    {
        public bool Success { get; set; }
        public string Total { get; set; }
        public Project[] Items;
    }

    public class Project
    {
        public int customerID { get; set; }
        public int projectID { get; set; }
        public string approved { get; set; }
        public string budget { get; set; }
        public string comment { get; set; }
        public string customerVisible { get; set; }
        public string effort { get; set; }
        public string filter { get; set; }
        public string @internal { get; set; }
        public string name { get; set; }
        public string trash { get; set; }
        public string visible { get; set; }
    }

    */

    /// <summary>
    /// Customer map
    /// </summary>
    public class CustomerMap
    {
        public string id { get; set; }
        public string jsonrpc { get; set; }
        public CustomerResult Result;
    }

    public class CustomerResult
    {
        public bool Success { get; set; }
        public string Total { get; set; }
        public Customer[] Items;
    }

    public class Customer
    {
        // Customer
        public string contact { get; set; }
        public int customerID { get; set; }
        public string name { get; set; }
        public string visible { get; set; }
    }

    /*
    /// <summary>
    /// Task map.
    /// </summary>
    public class TaskMap
    {
        public string id { get; set; }
        public string jsonrpc { get; set; }
        public TaskResult Result;
    }

    public class TaskResult
    {
        public bool Success { get; set; }
        public string Total { get; set; }
        public Task[] Items;
    }

    public class Task
    {
        public string activityId { get; set; }
        public string approved { get; set; }
        public string budget { get; set; }
        public string effort { get; set; }
        public string name { get; set; }
        public string visible { get; set; }
    }
    */
    public class RecordingMap
    {
        public string id { get; set; }
        public string jsonrpc { get; set; }
        public RecordingResult Result { get; set; }
    }

    public class RecordingResult
    {
        public bool Success { get; set; }
        public int Total { get; set; }
        public Recording[] Items { get; set; }
    }

    public class Recording
    {
        public string timeEntryID { get; set; }
        public string activityID { get; set; }
        public string projectID { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string duration { get; set; }
        public string servertime { get; set; }
        public string customerID { get; set; }
        public string customerName { get; set; }
        public string projectName { get; set; }
        public string activityName { get; set; }
    }

    public class ProjectMap
    {
        public string id { get; set; }
        public string jsonrpc { get; set; }
        public ProjectResult Result { get; set; }

        public class ProjectResult
        {
            public Project[] Items { get; set; }
            public bool Success { get; set; }
            public int total { get; set; }
        }
    }


    public class Project
    {
        public object approved { get; set; }
        public object budget { get; set; }
        public string comment { get; set; }
        public int customerID { get; set; }
        public string customerName { get; set; }
        public string customerVisible { get; set; }
        public object effort { get; set; }
        public string filter { get; set; }
        public string @internal { get; set; }
        public string name { get; set; }
        public int projectID { get; set; }
        public Task[] Tasks { get; set; }
        public string trash { get; set; }
        public string visible { get; set; }
    }

    public class Task
    {
        public int activityID { get; set; }
        public object approved { get; set; }
        public string budget { get; set; }
        public object effort { get; set; }
        public string name { get; set; }
        public string visible { get; set; }
    }



}






