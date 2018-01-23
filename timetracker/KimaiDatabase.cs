using System;
using SQLite;

namespace TimeTracker
{
    public class KimaiDatadase
    {
        public KimaiDatadase()
        {
        }
       
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

        public class ProjectActivity
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
    }
}
