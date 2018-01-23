using System;
using SQLite;

namespace TimeTracker
{
    /// <summary>
    /// Kimai datadase.
    /// </summary>
    public class KimaiDatadase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:TimeTracker.KimaiDatadase"/> class.
        /// </summary>
        public KimaiDatadase()
        {
        }
       
        /// <summary>
        /// Customer.
        /// </summary>
        public class Customer
        {
            [PrimaryKey][Unique]
            public int ID { get; set; }

            public string Name { get; set; }
            public override string ToString()
            {
                return string.Format("[Customer: ID={0}, Name={1}]", ID, Name);
            }
        }

        /// <summary>
        /// Project.
        /// </summary>
        public class Project
        {
            [PrimaryKey]
            [Unique]
            public int ID { get; set; }

            public string Name { get; set; }
            public int CustomerID { get; set; }
            public override string ToString()
            {
                return string.Format("[Project: ID={0},CustomerID{1}, Name={2}]", ID, CustomerID, Name);
            }
        }

        /// <summary>
        /// Project activity.
        /// </summary>
        public class ProjectActivity
        {
            [PrimaryKey]
            [Unique]
            public int ID { get; set; }
            public string Name { get; set; }
            public int ProjectID { get; set; }
            public override string ToString()
            {
                return string.Format("[Project: ID={0},Project{1}, Name={2}]", ID, ProjectID, Name);
            }
        }
    }
}
