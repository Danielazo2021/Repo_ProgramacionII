using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyCarpinteria.dominio
{
    class Presupuesto
    {
        public int PresupuestoNro { get; set; }
        public DateTime Fecha { get; set; }

        public string Cliente { get; set; }

        public double CostoMO { get; set; }
        public double Descuento { get; set; }

        public DateTime FechaBaja { get; set; }

        public List<DetallePresupuesto> Detalles { get; }

        public Presupuesto()
        {
            //generar la relación 1 a muchos
            Detalles = new List<DetallePresupuesto>();
        }


        public void AgregarDetalle(DetallePresupuesto detalle)
        {
            Detalles.Add(detalle);
        }

        public void QuitarDetalle(int nro)
        {
            Detalles.RemoveAt(nro);
        }


        public double CalcularTotal()
        {
            double total = 0;
            foreach(DetallePresupuesto item in Detalles)
            {
                total += item.CalcularSubTotal();
            }
            return total;
        }

        public bool Confirmar()
        {
            SqlTransaction transaccion = null;
            bool resultado = true;
            SqlConnection Cnn = new SqlConnection();

            try
            {

                Cnn.ConnectionString = "Data Source=LAPTOP-SJP45N95;Initial Catalog=carpinteria;Integrated Security=True";
                Cnn.Open();
                transaccion = Cnn.BeginTransaction();


                SqlCommand cmd = new SqlCommand("SP_INSERTAR_MAESTRO", Cnn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@cliente",Cliente);
                cmd.Parameters.AddWithValue("@dto", this.Descuento);
                cmd.Parameters.AddWithValue("@total", this.CalcularTotal() - this.Descuento);
                SqlParameter param = new SqlParameter("@presupuesto_nro",
                SqlDbType.Int);
                param.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param);
                cmd.ExecuteNonQuery();
                int presupuestoNro = Convert.ToInt32(param.Value);
                int cDetalles = 1;
                foreach (DetallePresupuesto det in Detalles)
                {
                    SqlCommand cmdDet = new SqlCommand("SP_INSERTAR_DETALLE", Cnn);
                    cmdDet.CommandType = CommandType.StoredProcedure;
                    cmdDet.Parameters.AddWithValue("@presupuesto_nro",
                    presupuestoNro);
                    cmdDet.Parameters.AddWithValue("@detalle", cDetalles);
                    cmdDet.Parameters.AddWithValue("@id_producto", det.Producto.IdProducto);
                    cmdDet.Parameters.AddWithValue("@cantidad", det.Cantidad);
                    cmdDet.ExecuteNonQuery();
                    cDetalles++;
                }
                transaccion.Commit();
            }
            catch (Exception ex)
            {
                transaccion.Rollback();
                resultado = false;
            }
            finally
            {
                if (Cnn != null && Cnn.State == ConnectionState.Open)
                    Cnn.Close();
            }
            return resultado;


        }
    }
}
