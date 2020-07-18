using System;
using System.Data;
using System.Data.SqlClient;

namespace Lg.BulkCopy
{
    public static class BulkCopy
    {
        public static void Inserir(DataTable tabela, string strConexao)
        {
            using (SqlConnection conexao = new SqlConnection(strConexao))
            {
                SqlBulkCopy bulkCopy = new SqlBulkCopy(
                conexao,
                SqlBulkCopyOptions.TableLock |
                SqlBulkCopyOptions.UseInternalTransaction |
                SqlBulkCopyOptions.CheckConstraints|
                SqlBulkCopyOptions.KeepIdentity
                ,null
                );

                foreach (dynamic item in tabela.Columns)
                {
                    bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(item.ColumnName, item.ColumnName));
                }

                bulkCopy.DestinationTableName = tabela.TableName;
                try
                {
                    conexao.Open();
                    bulkCopy.WriteToServer(tabela);
                    conexao.Close();
                }
                catch (Exception ex)
                {
                    conexao.Close();
                    throw ex;
                }
            }
        }
    }
}
