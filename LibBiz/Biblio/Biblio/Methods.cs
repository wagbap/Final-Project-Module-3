
using Cursos;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using Utilis;

namespace Biblio
{
    public class Methods
    {


        public bool GetCursos(ref DataSet cursoDS)
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(currentDirectory, "ConsoleADONET1.ini");
            Utils.INIFile ini = new Utils.INIFile(filePath);
            string cnString = ini.ReadKey("Connection_Biblio");
       

            // Removido já que cursoDS é passado como um parâmetro
            // DataSet cursoDS = new DataSet();

            SqlConnection cn = null;

            try
            {
                cn = BD.Abrir(cnString); ;
                string SQL = @"SELECT Cursos.ISBN, Cursos.nome AS Curso, Cursos.imagem AS FotoCapa, 
                       CONCAT(Autor.Nome, ' ', Autor.Apelido) AS Autor,
                       Categoria.nome AS Categoria, Cursos.preco AS Preço, Valor AS Avaliação, 
                       Promocao.Nome AS Promoção,  Promocao.ValorPercentagem AS Percentagem 
                       FROM Cursos 
                       INNER JOIN Autor ON Cursos.AutorID = Autor.id 
                       INNER JOIN Categoria ON Categoria.id = Cursos.CategoryID 
                       INNER JOIN Promocao ON Cursos.PromotionID = Promocao.id 
                       INNER JOIN Rating ON Cursos.RatingID = Rating.id";

                BD.FecharDS(ref cursoDS);
                cursoDS = BD.ExecutarSQL(cn, SQL);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                BD.Fechar(ref cn);
            }
              return cursoDS.Tables[0].Rows.Count > 0 ? true : false;
        }




        public class Venda
        {
            public DateTime DataVenda { get; set; }
            public int Quantidade { get; set; }
            public decimal ValorTotal { get; set; }
            public int CursoISBN { get; set; }

            public int UtilizadorID { get; set; }
        }

        public class VendaData
        {
            public List<Venda> Vendas { get; set; }
        }
        //Inserir uma Venda
        public bool InsertSale(VendaData vendaData)
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(currentDirectory, "ConsoleADONET1.ini");
            Utils.INIFile ini = new Utils.INIFile(filePath);
            string cnString = ini.ReadKey("Connection_Biblio");

            SqlConnection cn = null;
            SqlTransaction pTrans = null;
            bool isTransacao = false;
            bool result = false;

            try
            {
                cn = BD.Abrir(cnString);
                pTrans = cn.BeginTransaction();
                isTransacao = true;

                foreach (var venda in vendaData.Vendas)
                {
                    string insertSaleQuery = @"INSERT INTO Vendas (DataVenda, Quantidade, ValorTotal, CursoISBN,UtilizadorID ) 
                                      VALUES (@DataVenda, @Quantidade, @ValorTotal,  @CursoISBN, @UtilizadorID);
                                      SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmd = new SqlCommand(insertSaleQuery, cn, pTrans))
                    {
                        cmd.Parameters.AddWithValue("@DataVenda", venda.DataVenda);
                        cmd.Parameters.AddWithValue("@Quantidade", venda.Quantidade);
                        cmd.Parameters.AddWithValue("@ValorTotal", venda.ValorTotal);
                        cmd.Parameters.AddWithValue("@CursoISBN", venda.CursoISBN);
                        cmd.Parameters.AddWithValue("@UtilizadorID", venda.UtilizadorID);

                        object saleIdObj = cmd.ExecuteScalar();
                        if (saleIdObj != null && saleIdObj != DBNull.Value)
                        {
                            int saleId = Convert.ToInt32(saleIdObj);

                            string insertSaleDetailsQuery = @"INSERT INTO VendaCurso (VendaID, CursoISBN) 
                                                      VALUES (@VendaID, @CursoISBN);";

                            using (SqlCommand insertSaleDetailsCmd = new SqlCommand(insertSaleDetailsQuery, cn, pTrans))
                            {
                                insertSaleDetailsCmd.Parameters.Add("@VendaID", SqlDbType.Int);
                                insertSaleDetailsCmd.Parameters.Add("@CursoISBN", SqlDbType.Int);

                                insertSaleDetailsCmd.Parameters["@VendaID"].Value = saleId;
                                insertSaleDetailsCmd.Parameters["@CursoISBN"].Value = venda.CursoISBN;
                                insertSaleDetailsCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            throw new Exception("Falha ao obter o ID da venda gerado pelo banco de dados.");
                        }
                    }
                }

                pTrans.Commit();
                isTransacao = false;
                result = true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (isTransacao)
                {
                    try { pTrans.Rollback(); } catch { }
                }
                BD.Fechar(ref cn);
            }

            return result;
        }

        public bool GetCart(string userID, ref DataSet cartDS)
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(currentDirectory, "ConsoleADONET1.ini");
            Utils.INIFile ini = new Utils.INIFile(filePath);
            string cnString = ini.ReadKey("Connection_Biblio");

            SqlConnection cn = null;

            try
            {
                cn = BD.Abrir(cnString);
                if (userID != null)
                {
                    string SQL = $"SELECT * FROM Carinho WHERE Cliente_id = {userID}";
                    //bug here
                    if (BD.FecharDS != null && cartDS != null)
                    {
                        BD.FecharDS(ref cartDS);
                    }
                    cartDS = BD.ExecutarSQL(cn, SQL);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                BD.Fechar(ref cn);
            }
            return cartDS.Tables[0].Rows.Count > 0 ? true : false;
        }


        public int SetCart(string userID, string curso_ISBN, int quantidade, int total)
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(currentDirectory, "ConsoleADONET1.ini");
            Utils.INIFile ini = new Utils.INIFile(filePath);
            string cnString = ini.ReadKey("Connection_Biblio");


            SqlConnection cn = null;
            SqlTransaction pTrans = null;
            bool isTransacao = false;
            int result = 0;

            try
            {
                cn = BD.Abrir(cnString);
                pTrans = cn.BeginTransaction();
                isTransacao = true;

                string SQL = $"SELECT * FROM Carinho WHERE Curso_ISBN = '{curso_ISBN}' AND Cliente_id = '{userID}'";
                DataSet tempdt = BD.ExecutarSQL(cn, SQL, pTrans);
                if (tempdt.Tables[0].Rows.Count > 0 && quantidade > 0)
                {
                    // Existe um igual na base de dados, então vamos fazer update
                    SQL = $"UPDATE Carinho SET Quantidade = {quantidade}, Total = {total} WHERE Curso_ISBN = '{curso_ISBN}' AND Cliente_id = '{userID}'";
                    result = BD.ExecutarDDL(cn, SQL, pTrans);

                }
                else if (tempdt.Tables[0].Rows.Count > 0 && quantidade == 0)
                {
                    // Existe um igual na base de dados, com quantidade igual a 0 então vamos apagar
                    SQL = $"DELETE FROM Carinho WHERE Curso_ISBN = '{curso_ISBN}' AND Cliente_id = '{userID}'";
                    result = BD.ExecutarDDL(cn, SQL, pTrans);
                }
                else
                {
                    // Não existe igual na base de dados, então vamos adicionar
                    SQL = $"INSERT INTO Carinho (Curso_ISBN, Cliente_id, Quantidade, Total) VALUES ('{curso_ISBN}', '{userID}', {quantidade}, {total})";
                    result = BD.ExecutarDDL(cn, SQL, pTrans);
                }


                pTrans.Commit();
                isTransacao = false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (isTransacao)
                {
                    try { pTrans.Rollback(); } catch { }
                }
                BD.Fechar(ref cn);
            }

            return result;
        }


        //guardar os favoritos
        public bool GetFavoritos(ref DataSet favoritosDS, string Cliente_id)
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(currentDirectory, "ConsoleADONET1.ini");
            Utils.INIFile ini = new Utils.INIFile(filePath);
            string cnString = ini.ReadKey("Connection_Biblio");


            SqlConnection cn = null;

            try
            {
                cn = BD.Abrir(cnString);

                string SQL = "SELECT * FROM Favoritos WHERE Cliente_id = '" + Cliente_id + "'";
                //bug here
                if (BD.FecharDS != null && favoritosDS != null)
                {
                    BD.FecharDS(ref favoritosDS);
                }

                favoritosDS = BD.ExecutarSQL(cn, SQL);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                BD.Fechar(ref cn);
            }
            return favoritosDS.Tables[0].Rows.Count > 0 ? true : false;
        }


        public int SetFavoritos(string Cliente_id, string curso_ISBN)
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(currentDirectory, "ConsoleADONET1.ini");
            Utils.INIFile ini = new Utils.INIFile(filePath);
            string cnString = ini.ReadKey("Connection_Biblio");


            SqlConnection cn = null;
            SqlTransaction pTrans = null;
            bool isTransacao = false;
            int result = 0;

            try
            {
                cn = BD.Abrir(cnString);
                pTrans = cn.BeginTransaction();
                isTransacao = true;

                string SQL = $"SELECT * FROM Favoritos WHERE Curso_ISBN = '{curso_ISBN}' AND Cliente_id = '{Cliente_id}'";
                DataSet tempdt = BD.ExecutarSQL(cn, SQL, pTrans);
                if (tempdt.Tables[0].Rows.Count > 0)
                {
                    // Existe um favorito igual na base de dados, então vamos apagar
                    SQL = $"DELETE FROM Favoritos WHERE Curso_ISBN = '{curso_ISBN}' AND Cliente_id = '{Cliente_id}'";
                    BD.ExecutarDDL(cn, SQL, pTrans);
                }
                else
                {
                    // Não existe um favorito igual na base de dados, então vamos adicionar
                    SQL = $"INSERT INTO Favoritos (Curso_ISBN, Cliente_id) VALUES ('{curso_ISBN}', '{Cliente_id}')";
                    result = BD.ExecutarDDL(cn, SQL, pTrans);
                }

                pTrans.Commit();
                isTransacao = false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (isTransacao)
                {
                    try { pTrans.Rollback(); } catch { }
                }
                BD.Fechar(ref cn);
            }

            return result;
        }



        // delet all item from cart
        public int ClearCart(string user)
          {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(currentDirectory, "ConsoleADONET1.ini");
            Utils.INIFile ini = new Utils.INIFile(filePath);
            string cnString = ini.ReadKey("Connection_Biblio");


            SqlConnection cn = null;
            SqlTransaction pTrans = null;
            bool isTransacao = false;
            int result = 0;

            try
            {
                cn = BD.Abrir(cnString);
                pTrans = cn.BeginTransaction();
                isTransacao = true;

                string SQL = $"DELETE FROM Carinho WHERE Cliente_id = '{user}'";
                result = BD.ExecutarDDL(cn, SQL, pTrans);

                pTrans.Commit();
                isTransacao = false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (isTransacao)
                {
                    try { pTrans.Rollback(); } catch { }
                }
                BD.Fechar(ref cn);
            }
            return result;
        }

        public int finalizarCompra(string userID)
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(currentDirectory, "ConsoleADONET1.ini");
            Utils.INIFile ini = new Utils.INIFile(filePath);
            string cnString = ini.ReadKey("Connection_Biblio");

            SqlConnection cn = null;
            SqlTransaction pTrans = null;
            bool isTransacao = false;
            int result = 0;

            try
            {
                cn = BD.Abrir(cnString);
                pTrans = cn.BeginTransaction();
                isTransacao = true;

                // Buscar os itens do carrinho pertencentes ao usuário
                string selectCartItemsSQL = $"SELECT * FROM Carinho WHERE Cliente_id = {userID}";
                DataSet cartItemsDS = new DataSet();
                BD.FecharDS(ref cartItemsDS); // Close any existing DataSet
                cartItemsDS = BD.ExecutarSQL(cn, selectCartItemsSQL, pTrans);

                // Get the DataTable from the DataSet
                DataTable cartItemsTable = cartItemsDS.Tables[0];

                // Percorrer os itens do carrinho e inserir na tabela "Vendidos"
                foreach (DataRow cartItem in cartItemsTable.Rows)
                {
                    int isbn = Convert.ToInt32(cartItem["Curso_ISBN"]);
                    int quantidade = Convert.ToInt32(cartItem["Quantidade"]);
                    decimal total = Convert.ToDecimal(cartItem["Total"]);

                    string dataVenda = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    string insertVendidosSQL = $"INSERT INTO Vendas (DataVenda, Cliente_id, Curso_ISBN, Quantidade, ValorTotal) VALUES ('{dataVenda}', {userID}, {isbn}, {quantidade}, {total})";
                    result = BD.ExecutarDDL(cn, insertVendidosSQL, pTrans);

                    // Aqui você pode fazer outras operações com os itens do carrinho se necessário

                    // Por exemplo, remover os itens do carrinho após inserir em "Vendidos"
                    string deleteCartItemSQL = $"DELETE FROM Carinho WHERE Cliente_id = {userID} AND Curso_ISBN = {isbn}";
                    BD.ExecutarDDL(cn, deleteCartItemSQL, pTrans);
                }

                pTrans.Commit();
                isTransacao = false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (isTransacao)
                {
                    try { pTrans.Rollback(); } catch { }
                }
                BD.Fechar(ref cn);
            }
            return result;
        }


        public bool getSolds(ref DataSet vendidoDS, ref DataSet top5)
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(currentDirectory, "ConsoleADONET1.ini");
            Utils.INIFile ini = new Utils.INIFile(filePath);
            string cnString = ini.ReadKey("Connection_Biblio");

            SqlConnection cn = null;


            try
            {
                cn = BD.Abrir(cnString);

                string SQL = $"SELECT * FROM Vendas";
                //bug here
                if (BD.FecharDS != null && vendidoDS != null)
                {
                    BD.FecharDS(ref vendidoDS);
                }
                vendidoDS = BD.ExecutarSQL(cn, SQL);

                // Criar um dicionário para armazenar a quantidade total de livros vendidos por ISBN
                Dictionary<int, int> soldCounts = new Dictionary<int, int>();

                // Calcular a quantidade total de livros vendidos para cada ISBN
                if (vendidoDS.Tables.Count > 0)
                {
                    DataTable table = vendidoDS.Tables[0];
                    int rowCount = table.Rows.Count;

                    for (int i = 0; i < rowCount; i++)
                    {
                        DataRow row = table.Rows[i];
                        int ISBN = Convert.ToInt32(row["Curso_ISBN"]);
                        int quantidade = Convert.ToInt32(row["Quantidade"]);

                        // Verificar se o ISBN já existe no dicionário
                        if (soldCounts.ContainsKey(ISBN))
                        {
                            soldCounts[ISBN] += quantidade;
                        }
                        else
                        {
                            soldCounts[ISBN] = quantidade;
                        }
                    }

                    // Classificar o dicionário com base na quantidade de livros vendidos (ordem decrescente)
                    var sortedSoldCounts = soldCounts.OrderByDescending(pair => pair.Value).Take(5);

                    // Criar uma nova DataTable para armazenar os top 5 cursos mais vendidos
                    DataTable top5Table = new DataTable();
                    top5Table.Columns.Add("Curso_ISBN", typeof(int));
                    top5Table.Columns.Add("Quantidade", typeof(int));

                    // Adicionar os top 5 registros à DataTable
                    foreach (var item in sortedSoldCounts)
                    {
                        top5Table.Rows.Add(item.Key, item.Value);
                    }

                    // Criar um novo DataSet para armazenar os top 5 cursos mais vendidos
                    top5 = new DataSet();
                    top5.Tables.Add(top5Table);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                BD.Fechar(ref cn);
            }

            return vendidoDS.Tables[0].Rows.Count > 0 ? true : false;
        }

        public DataSet GetCursos()
        {
            throw new NotImplementedException();
        }
    }
}