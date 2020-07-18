using Lg.BulkCopy.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;

namespace Lg.BulkCopy
{
    public static class MapeadorDeTabela<T> where T : IBulkCopyMapTable
    {
        private static DataTable _tabela { get; set; }

        /// <summary>
        /// Mapeia qualquer objeto que herde de IBulkMapTable utilizando o nome das propriedades ou utilizando o ColumnAttribute.
        /// </summary>
        /// <param name="lista">Lista de objetos do tipo genérico que herde de IBulkMapTable.</param>
        /// <param name="nomeTabela">Nome da tabela que será realizado o mapeamento.</param>
        /// <returns>Retorna um objeto DataTable com as colunas mapeadas e com as linhas da tabela preenchidas com os valores das propriedades dos objetos da lista genérica
        /// passada por parâmetro.
        /// </returns>
        public static DataTable MapearTabela(List<T> lista, string nomeTabela)
        {
            _tabela = new DataTable();
            _tabela.TableName = nomeTabela;
            ObterColunas();
            ObterLinhas(lista);
            return _tabela;
        }

        private static void ObterLinhas(List<T> lista)
        {
            foreach (var item in lista)
            {
                var propriedades = item.GetType().GetProperties();
                var parametrosDaLinha = ObterParametrosDaLinha(item, propriedades);
                _tabela.Rows.Add(parametrosDaLinha);
            }
        }

        private static object[] ObterParametrosDaLinha(T item, PropertyInfo[] propriedades)
        {
            List<object> parametrosLinha = new List<object>();
            foreach (var propriedade in propriedades)
            {
                var atributos = propriedade.GetCustomAttributes(true);
                if (DeveMapearPropriedade(atributos))
                {
                    parametrosLinha.Add(propriedade.GetValue(item));
                }
            }
            return parametrosLinha.ToArray();
        }

        private static void ObterColunas()
        {
            List<DataColumn> listaColunas = new List<DataColumn>();

            var propriedadesClasseGenerica = typeof(T).GetProperties();
            foreach (var propriedade in propriedadesClasseGenerica)
            {
                var atributos = propriedade.GetCustomAttributes(true);
                if (DeveMapearPropriedade(atributos))
                {
                    string nomeColuna = ObterNomeDaColuna(propriedade, atributos);
                    AdicionarColuna(propriedade, nomeColuna);
                }
            }
        }

        private static void AdicionarColuna(PropertyInfo propriedade, string nomeColuna)
        {
            var tipoDaPropriedade = Type.GetType(propriedade.PropertyType.FullName);
            bool adicionarColuna = ValidarTipoDaColuna(propriedade, ref tipoDaPropriedade);
            if (adicionarColuna)
            {
                DataColumn coluna = new DataColumn(nomeColuna, tipoDaPropriedade);
                _tabela.Columns.Add(coluna);
            }
        }

        private static bool ValidarTipoDaColuna(PropertyInfo propriedade, ref Type tipoDaPropriedade)
        {
            bool adicionarColuna = true;
            if (tipoDaPropriedade == null)
            {
                tipoDaPropriedade = Type.GetType(propriedade.PropertyType.BaseType.FullName);
                adicionarColuna = tipoDaPropriedade != null ? true : false;
            }
            else
            {
                if (tipoDaPropriedade.IsGenericType && tipoDaPropriedade.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    tipoDaPropriedade = Nullable.GetUnderlyingType(tipoDaPropriedade);
                }
            }
            return adicionarColuna;
        }

        private static bool DeveMapearPropriedade(object[] atributos)
        {
            foreach (var atributo in atributos)
            {
                var tipo = atributo.GetType();
                if (tipo == typeof(NotMappedAttribute))
                {
                    return false;
                }
            }
            return true;
        }

        private static string ObterNomeDaColuna(PropertyInfo propriedade, object[] atributos)
        {
            string nomeColuna = propriedade.Name;
            foreach (var atributo in atributos)
            {
                var tipo = atributo.GetType();
                if (tipo == typeof(ColumnAttribute))
                {
                    nomeColuna = ((ColumnAttribute)(atributo)).Name ?? propriedade.Name;
                }
            }

            return nomeColuna;
        }
    }
}
