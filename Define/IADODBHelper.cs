﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Define
{
    public interface IADODBHelper
    {
        void BeginTrans();   
        void Commit(); 
        void Rollback();

        int ExecuteSQL(string strSql);
        DataSet ExecuteDataset(string strSql);
        DataTable ExecuteDataTable(string strSql);
        object ExecuteScalar(string strSql);
        IDataReader ExecuteReader(string strSql);

        DataTable OpenTable(string strTable);

        bool TableExists(string strTable);


        IDbConnection ADOConnection { get; }
    }
}
