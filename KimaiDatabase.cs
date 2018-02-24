using System;
using SQLite;

namespace Kimai
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
            [PrimaryKey][AutoIncrement]
            public int ID { get; set; } // an auto increment data base ID

            public int CustomerID { get; set; } // the Customer ID as stored in Kimai 
            public string Name { get; set; }


            public override string ToString()
            {
                return string.Format("[Customer: ID={0}, CustomerID={1}, Name={2}]", ID, CustomerID, Name);
            }
        }

        /// <summary>
        /// Project.
        /// </summary>
        public class Project
        {
            [PrimaryKey][AutoIncrement]
            public int ID { get; set; } // an auto increment database Id

            public int ProjectID { get; set;} // the Project ID as stored in Kimai
            public int CustomerID { get; set; }// the Customer ID foregin key
            public string Name { get; set; } 

            public override string ToString()
            {
                return string.Format("[Project: ID={0},ProjectID={1}, CustomerID={2}, Name={3}]", ID, ProjectID, CustomerID, Name);
            }
        }

        /// <summary>
        /// Project activity.
        /// </summary>
        public class ProjectActivity
        {
            [PrimaryKey][AutoIncrement]
            public int ID { get; set; } // an auto increment database Id
            public int ActivityID { get; set; } // Activity ID as Stored in Kimai
            public int ProjectID { get; set; } // Project ID foreign key as stored in Kimai
            public string Name { get; set; }

            public override string ToString()
            {
                return string.Format("[ProjectActivity: ID={0}, ActivityID={1},Project={2}, Name={3}]", ID, ActivityID, ProjectID, Name);
            }
        }
    }
}
