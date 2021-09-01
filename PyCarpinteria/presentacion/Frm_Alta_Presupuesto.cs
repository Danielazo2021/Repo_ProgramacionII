using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PyCarpinteria.dominio;

namespace PyCarpinteria.presentacion
{
    public partial class Frm_Alta_Presupuesto : Form
    {
        private Presupuesto oPresupuesto;

        public Frm_Alta_Presupuesto()
        {
            InitializeComponent();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (txtCliente.Text == "")
            {
                MessageBox.Show("Debe ingresar un cliente!", "Control", MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
                return;
            }
            if (dgvDetalles.Rows.Count == 0)
            {
                MessageBox.Show("Debe ingresar al menos detalle!",
                "Control", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            GuardarPresupuesto();
        }

        private void GuardarPresupuesto()
        {
            oPresupuesto.Cliente = txtCliente.Text;
            oPresupuesto.Descuento = Convert.ToDouble(txtDto.Text);
            oPresupuesto.Fecha = Convert.ToDateTime(txtFecha.Text);
            if (oPresupuesto.Confirmar())
            {
                MessageBox.Show("Presupuesto registrado", "Informe", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Dispose();
            }
            else
            {
                MessageBox.Show("ERROR. No se pudo registrar el presupuesto", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Frm_Alta_Presupuesto_Load(object sender, EventArgs e)
        {

            ConsultarUltimoPresupuesto();
            CargarCombo();

            //Valores por defecto:
            txtFecha.Text =  DateTime.Now.ToString("dd/MM/yyyy");
            txtCliente.Text = "CONSUMIDOR FINAL";
            txtDto.Text = "0";
            cboProducto.DropDownStyle = ComboBoxStyle.DropDownList;

            //Crear un nuevo objeto Presupuesto:
            oPresupuesto = new Presupuesto();
        }

        private void CargarCombo()
        {
            SqlConnection cnn = new SqlConnection("Data Source=LAPTOP-SJP45N95;Initial Catalog=carpinteria;Integrated Security=True");
            cnn.Open();
                //Command productos:
                DataTable tabla = new DataTable();
                SqlCommand cmd2 = new SqlCommand("SP_CONSULTAR_PRODUCTOS", cnn);
                cmd2.CommandType = CommandType.StoredProcedure;
                tabla.Load(cmd2.ExecuteReader());
                cboProducto.DataSource = tabla;
                cboProducto.DisplayMember = tabla.Columns[1].ColumnName;
                cboProducto.ValueMember = tabla.Columns[0].ColumnName;
            cnn.Close();

        }

        private void ConsultarUltimoPresupuesto()
        {
            SqlConnection cnn = new SqlConnection("Data Source=LAPTOP-SJP45N95;Initial Catalog=carpinteria;Integrated Security=True");
            cnn.Open();

                //command proximo ID
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = cnn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_PROXIMO_ID";
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@next";
                param.SqlDbType = SqlDbType.Int;
                param.Direction = ParameterDirection.Output;

                cmd.Parameters.Add(param);
                cmd.ExecuteNonQuery();
                lblNro.Text = "Presupuesto Nro: " + param.Value.ToString();

            cnn.Close();

        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {

            if (ExisteProductoEnGrilla(cboProducto.Text)) {
                MessageBox.Show("Producto ya agregado como detalle", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;           
            }

            
            DialogResult result = MessageBox.Show("Desea Agregar?", "Confirmación", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                DetallePresupuesto item = new DetallePresupuesto();
                item.Cantidad = (int) nudCantidad.Value;
                DataRowView oDataRow = (DataRowView)cboProducto.SelectedItem;
                //Producto:
                Producto oProducto = new Producto();
                oProducto.IdProducto =  Int32.Parse(oDataRow[0].ToString());
                oProducto.Nombre =  oDataRow[1].ToString();
                oProducto.Precio =  Double.Parse(oDataRow[2].ToString());
                item.Producto = oProducto;

                oPresupuesto.AgregarDetalle(item);

                dgvDetalles.Rows.Add(new object[] { "", oProducto.Nombre, oProducto.Precio, item.Cantidad, item.CalcularSubTotal() });

                CalcularTotales();   
            
            }

        }

        private bool ExisteProductoEnGrilla(string producto)
        {
            foreach(DataGridViewRow row in dgvDetalles.Rows)
            {
                string col = row.Cells["producto"].Value.ToString();
                if (col.Equals(producto))
                    return true;
            }

            return false;
        }

        private void CalcularTotales()
        {
            double subTotal = oPresupuesto.CalcularTotal();
            double desc = (Double.Parse(txtDto.Text) * subTotal) /100;
            double total = subTotal - desc;

            lblSubTotal.Text = "SubTotal: " + subTotal.ToString();
            lblDto.Text = "Descuento:" + desc.ToString();
            lblTotal.Text = "Total:" + total.ToString();
        }

        private void dgvDetalles_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvDetalles.CurrentCell.ColumnIndex == 5) {

                oPresupuesto.QuitarDetalle(dgvDetalles.CurrentRow.Index);
                dgvDetalles.Rows.Remove(dgvDetalles.CurrentRow);
                CalcularTotales();
            }
        }
    }
}
