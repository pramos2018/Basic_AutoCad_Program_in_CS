using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace NS_BasicAutoCadApp
{
    public partial class BasicAutoCadApp : Form
    {
        //Project Variables
        Graphics g1, g2;
        Color dColor;
        Pen dPen;
        Brush dBrush;
        Font dFont;
        int size_h, size_w, dsize = 50;
        String selShape = "", selItem = "";
        Point lp1, lp2, p1, p2;

        string CurrFile = "";
        Bitmap currBmp;
        Image currImg;
        bool flagImg = false;

        bool flagPaint = false, flagFill = false, flagP1 = false;
        // AutoCad Project Variables
        List<shapeX> compList;
        int rotation = 0, selectedShape = -1;
        bool flagSelect = false, flagCopy = false, flagPaste = false, flagCut = false;
        shapeX currShape = null;
        float zoom = 1.0f;
        bool flagZoom = false;
        int IC_Start = 30;
        String dText = "", currFile="";


        public BasicAutoCadApp()
        {
            InitializeComponent();
        }

        private void Form6_Load(object sender, EventArgs e)
        {
            start();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }



        //***************** Code *********************************//
        private void start()
        {

            int pw = panel1.Width;
            int ph = panel1.Height;

            currBmp = new Bitmap(pw, ph);
            g2 = Graphics.FromImage(currBmp);
            g1 = panel1.CreateGraphics();

            dsize = 50; size_h = 50; size_w = 50;

            //-----------------------------
            penSize.Value = 1;
            //------------------------------

            dColor = Color.Black;
            dBrush = new SolidBrush(dColor);
            dPen = new Pen(dBrush, 1);
            flagPaint = false;
            flagFill = false;
            flagP1 = true;
            selShape = "Rectangle";
            createDropDown();
            dFont = txtInput.Font;
            clearScreen();
            startAutoCad();

        }
        private void createDropDown()
        {
            cbox1.Items.Clear();
            cbox1.Items.Add("Drawing");
            cbox1.Items.Add("Rectangle (p1,p2)");
            cbox1.Items.Add("Ellipse (p1,p2)");
            cbox1.Items.Add("Square");
            cbox1.Items.Add("Rectangle (H)");
            cbox1.Items.Add("Rectangle (V)");
            cbox1.Items.Add("Circle");
            cbox1.Items.Add("Line");
            cbox1.Items.Add("Text");
            cbox1.Items.Add("Erase");
            cbox1.Items.Add("Erase Range");
            cbox1.SelectedIndex = 0;
            setShape();

        }
        private void setShape()
        {
            selItem = cbox1.SelectedItem.ToString();
            //MessageBox.Show(selItem);

            //dsize = Convert.ToInt32(txtSize.Text);
            size_w = dsize; size_h = dsize;
            selShape = selItem;
            flagP1 = true;
            lblStatus.Text = "Selected Shape: " + selShape;

            MoveGraphics();
            rotation = 0;
            flagSelect = false;
            setEdit("");
        }


        private void resizePanel()
        {
            try
            {
                Bitmap tmp = currBmp;
                int pw = currBmp.Width;
                int ph = currBmp.Height;

                if (ph < panel1.Height) ph = panel1.Height;
                if (pw < panel1.Width) pw = panel1.Width;

                currBmp = new Bitmap(pw, ph);
                g2 = Graphics.FromImage(currBmp);
                g1 = panel1.CreateGraphics();

                clearScreen();
                g1.DrawImage(tmp, new Point(0, 0));
                MoveGraphics();
            }
            catch (Exception ex) { }
            //MessageBox.Show("Window Resized!");
        }
        private Bitmap CopyBitmap(Bitmap srcBitmap, Rectangle section)
        {
            //Routine from MSDN
            // Create the new bitmap and associated graphics object
            Bitmap bmp = new Bitmap(section.Width, section.Height);
            Graphics g3 = Graphics.FromImage(bmp);

            // Draw the specified section of the source bitmap to the new one
            g3.DrawImage(srcBitmap, 0, 0, section, GraphicsUnit.Pixel);

            // Clean up
            g3.Dispose();

            // Return the bitmap
            return bmp;
        }

        private void MoveGraphics()
        {
            g1.DrawImage(currBmp, 0, 0);
            if (flagZoom == true) showZoom();
        }
        private void setPaste()
        {
            selShape = "Paste Image";
            flagP1 = true;
            lblStatus.Text = "Selected Shape: " + selShape;
            try
            {
                flagImg = true;
                currImg = Clipboard.GetImage();
            }
            catch (Exception ex)
            {
                flagImg = false;
            };
        }


        //*********************[Panel Events]*********************//
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (selShape == "Drawing")
            {
                flagPaint = true;
            }
            p1 = new Point(e.X, e.Y);
        }
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            addShapeX(e.X, e.Y);//Testing
            //MoveGraphics();// g1 - BitMap, g2 - Panel
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            showShapeX(e.X, e.Y);
        }
        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            MoveGraphics();
        }

        //**************** Graphical User Interface **************//

        private void shapDropBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            setShape();
        }


        private void clearScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clearScreen();
        }

        private void penSize_ValueChanged(object sender, EventArgs e)
        {
            int ps = Convert.ToInt32(penSize.Value);
            dPen = new Pen(dColor, ps);
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
            {
                dColor = cd.Color;
                int ps = Convert.ToInt32(penSize.Value);
                dPen = new Pen(dColor, ps);
                dBrush = new SolidBrush(dColor);
                btnColor.ForeColor = cd.Color;
            }

        }

        private void btnFont_Click(object sender, EventArgs e)
        {
            FontDialog fd = new FontDialog();
            if (fd.ShowDialog() == DialogResult.OK)
            {
                dFont = fd.Font;
                txtInput.Font = fd.Font;
                txtInput.Font = new Font(txtInput.Font.FontFamily, 10);
            }
        }

        private void newFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrFile = "";
            clearScreen();
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void saveFileAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }

        private void btnCut_Click(object sender, EventArgs e)
        {
            setEdit("Cut");
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            setEdit("Copy");
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            setEdit("Paste");
        }


        private void txtSize_MouseLeave(object sender, EventArgs e)
        {
            setShape();
        }

        private void Form6_ResizeEnd(object sender, EventArgs e)
        {
        }

        private void panel1_Resize(object sender, EventArgs e)
        {
            resizePanel();
        }

        //********************************************************//

//*************************************************************************************
//*********************[ AUTOCAD ROUTINES ]********************************************
        private void startAutoCad()
        {
            compList = new List<shapeX>();
            createDropDownAutoCad();

        }
        private void createDropDownAutoCad()
        {

            cbox1.Items.Clear();
            cbox1.Items.Add("Line");
            cbox1.Items.Add("Rectangle");
            cbox1.Items.Add("Circle");
            cbox1.Items.Add("Text");
            cbox1.Items.Add("***************");
            cbox1.Items.Add("Wire");
            cbox1.Items.Add("Resistor");
            cbox1.Items.Add("Capacitor");
            cbox1.Items.Add("Electrolytic Capacitor");
            cbox1.Items.Add("Diode");
            cbox1.Items.Add("LED");
            cbox1.Items.Add("LDR");

            cbox1.Items.Add("Transistor NPN");
            cbox1.Items.Add("Transistor PNP");
            cbox1.Items.Add("Variable Resistor");
            cbox1.Items.Add("Variable Capacitor");

            cbox1.Items.Add("Positive");
            cbox1.Items.Add("Negative");
            cbox1.Items.Add("Ground");
            cbox1.Items.Add("Battery");
            cbox1.Items.Add("Speaker");
            cbox1.Items.Add("Connector");
            cbox1.Items.Add("Dot");

            cbox1.Items.Add("Inductor 2 pins");
            cbox1.Items.Add("Inductor 3 pins");
            cbox1.Items.Add("Small Inductor");
            cbox1.Items.Add("Relay");
            cbox1.Items.Add("Transformer");
            cbox1.Items.Add("RF Coil");




            cbox1.Items.Add("***************");
            cbox1.Items.Add("IC 2 pins");
            cbox1.Items.Add("IC 4 pins");
            cbox1.Items.Add("IC 6 pins");
            cbox1.Items.Add("IC 8 pins");
            cbox1.Items.Add("IC 10 pins");
            cbox1.Items.Add("IC 12 pins");
            cbox1.Items.Add("IC 14 pins");
            cbox1.Items.Add("IC 16 pins");
            cbox1.Items.Add("IC 18 pins");
            cbox1.Items.Add("IC 20 pins");
            cbox1.Items.Add("IC 22 pins");
            cbox1.Items.Add("IC 24 pins");
            cbox1.Items.Add("IC 26 pins");
            cbox1.Items.Add("IC 28 pins");
            cbox1.Items.Add("IC 30 pins");
            cbox1.Items.Add("IC 32 pins");
            cbox1.Items.Add("IC 34 pins");
            cbox1.Items.Add("IC 36 pins");
            cbox1.Items.Add("IC 38 pins");
            cbox1.Items.Add("IC 40 pins");
            cbox1.Items.Add("IC 42 pins");
            cbox1.Items.Add("IC 44 pins");
            cbox1.Items.Add("IC 46 pins");
            cbox1.Items.Add("IC 48 pins");
            cbox1.Items.Add("IC 50 pins");
            cbox1.Items.Add("***************");

            cbox1.SelectedIndex = 0;
            setShape();
        }

        private void drawComponentX(Graphics g, String str, int x, int y, int rot) {

            int dx1, dy1, dx2, dy2, x1, y1, x2, y2, n, ctr, i, a1, a2;
            Point px1, px2;
            String sp = "L";
	
            //g.setColor(dColor);//Pen and SolidBrush Color
            //g.setStroke(new BasicStroke(penSize));


            String [] strx = str.Replace(" ", "").Split(';');
	
            n = Convert.ToInt32(strx[0]);
            ctr = 1;
            for (i=0; i<n;i++) {
                //---------------- Reading the String -------------------------------
                sp = strx[ctr+0];
                dx1 = Convert.ToInt32(strx[ctr+1]);	dy1 = Convert.ToInt32(strx[ctr+2]);
                dx2 = Convert.ToInt32(strx[ctr+3]);	dy2 = Convert.ToInt32(strx[ctr+4]);
                ctr = ctr + 5;
                //---------------- Rotation -----------------------------------------
                x1 = x + dx1; y1 = y + dy1; x2 = x + dx2; y2 = y + dy2;a1 = 270; a2=90;//default
                if (rot==1) {x1 = x + dy1; y1 = y + dx1; x2 = x + dy2; y2 = y + dx2;a1=180; a2=0;}
                if (rot==2) {x1 = x - dx1; y1 = y - dy1; x2 = x - dx2; y2 = y - dy2;a1=90; a2 = 270;}
                if (rot==3) {x1 = x - dy1; y1 = y - dx1; x2 = x - dy2; y2 = y - dx2;a1=0; a2 = 180;}
                //--------------- px2 always > px1 ----------------------------------
                px2 = new Point(x2, y2); px1 = new Point(x1, y1);//Default
                if (y2>y1 && x2 > x1) {px2 = new Point(x2, y2); px1 = new Point(x1, y1);}
                if (y2>y1 && x1 > x2) {px2 = new Point(x1, y2); px1 = new Point(x2, y1);}
                if (y1>y2 && x1 > x2) {px2 = new Point(x1, y1); px1 = new Point(x2, y2);}
                if (y1>y2 && x2 > x1) {px2 = new Point(x2, y1); px1 = new Point(x1, y2);}
                if (sp.Equals("L")) {px2 = new Point(x2, y2); px1 = new Point(x1, y1);}

                Pen pen = dPen;
                //--------------- Basic Shapes ---------------------------------------
                if (sp.Equals("L")) {g.DrawLine(pen, px1.X, px1.Y, px2.X, px2.Y);}//Line
                if (sp.Equals("R")) {g.DrawRectangle(pen, px1.X, px1.Y, px2.X-px1.X,px2.Y-px1.Y);}//Rectangle
                if (sp.Equals("C")) {g.DrawEllipse(pen, px1.X, px1.Y, px2.X - px1.X, px2.Y - px1.Y);}//Circle
                if (sp.Equals("A")) {g.DrawArc(pen, px1.X, px1.Y, px2.X-px1.X, px2.Y - px1.Y, a1, 180);}
                if (sp.Equals("IA")) {g.DrawArc(pen, px1.X, px1.Y, px2.X-px1.X, px2.Y - px1.Y, a2, 180);}
                //--------------------------------------------------------------------
            }
        }
        private void clearScreen()
        {
            g2.Clear(panel1.BackColor);
            try
            {
                //compList.RemoveAll();
                compList.Clear();
            }
            catch (Exception ex) { };

            MoveGraphics();//Moves g2(BitMap) to g1(Panel)
        }
        private void clearPanel()
        {
            g2.Clear(panel1.BackColor);
        }

        private void updateScreen() {
            clearPanel();
            shapeX t;
            Point bklp1 = lp1;//backup
            for (int i = 0; i < compList.Count();i++) {
                t = compList[i];
                drawComponent(g2, t.shape, t.x, t.y, t.rot, t.lpx, false, t.txt);
            }
            MoveGraphics();
            lp1 = bklp1;//Restore
        }

        private void showShapeX(int x, int y) {
            int comp = cbox1.SelectedIndex;
            dText = txtInput.Text.ToString();
            MoveGraphics();
	
            if (flagCopy == true || flagCut == true) return;
            if (flagPaste=true && selectedShape != -1) {
                    shapeX t;
                    t = currShape;
                    dColor = Color.Blue;
                    drawComponent(g1, t.shape, x, y, t.rot, t.lpx, false, t.txt);
                    dColor = Color.Black;
            } else {
                drawComponent(g1, comp, x, y, rotation, lp1, flagP1, dText);
            }
        }
        

private void addShapeX(int x, int y) {
    int comp = cbox1.SelectedIndex;
    if (flagPaste==true && selectedShape != -1) {
            shapeX t = new shapeX(currShape.shape, x, y, currShape.rot);
            t.lpx = currShape.lpx;  t.txt = currShape.txt;
            compList.Add(t); updateScreen();
            pasteShape(); flagPaste=false; selectedShape = -1; flagSelect = false; return;
        }
    if (flagSelect==true) {SelectShape(x, y);}
    if (flagCopy==true && selectedShape != -1) {copyShape();return;}
    if (flagCut==true && selectedShape != -1) {cutShape();return;}
	
	
    if (comp==0 || comp==1 || comp==2 || comp==5){
        if (flagP1 == true){
            lp1 = new Point(x, y); flagP1 = false;
        } else{
            shapeX t = new shapeX(comp, x, y, rotation);
            t.lpx = lp1;
            compList.Add(t);
            updateScreen();
            flagP1 = true;
        }
    } else {
        shapeX t = new shapeX(comp, x, y, rotation);
        if (comp == 3) t.txt = txtInput.Text;
        compList.Add(t);
        updateScreen();
    }
	
}
private void drawComponent(Graphics g, int comp, int x, int y, int rot, Point lpz, bool flagPt, String lText) {
    String str = "";
    int dx, dy;
	
    if (comp==0 || comp==1 || comp==2 || comp==5){
        if (flagPt == false){
            dx = lpz.X - x; dy = lpz.Y - y;
            String strXY = Convert.ToString(dx) + ";" +  Convert.ToString(dy)+ ";";
            if ((lpz.X - x != 0 ) || (lpz.Y - y != 0 )) {
            	
                if (comp==0) {str = "1;L;0;0;"+ strXY;}//Line
                if (comp==1) {str = "1;R;0;0;"+ strXY;}//Rectangle
                if (comp==2) {str = "1;C;0;0;"+ strXY;}//Circle
            	
                if (comp==5) {
                    String strXY1 = Convert.ToString(dx-1) + ";" +  Convert.ToString(dy-1)+ ";";
                    String strXY2 = Convert.ToString(dx+1) + ";" +  Convert.ToString(dy+1)+ ";";
                    str = "3; R;-1;-1;1;1; L;0;0;"+ strXY+ " R;"+strXY1+ strXY2; 
                    }//Wire
            }
        }
    } //Wire	
    if (comp==6) {str = "3; L;0;0;12;0; R;12;4;36;-4; L;36;0;48;0;";}  //Resistor
    if (comp==7) {str = "4; L;0;0;12;0; L;12;8;12;-8; L;18;8;18;-8; L;18;0;30;0;";}  //Capacitor
    if (comp==8) {str = "7; L;0;0;12;0; L;10;8;10;-8; L;12;8;12;-8; L;18;8;18;-8; L;18;0;30;0; L;20;-4;26;-4; L;23;-2;23;-8;";}  //Electrolytic Capacitor
    if (comp==9) {str = "6; L;0;0;12;0; L;12;6;12;-6; L;12;6;24;0; L;12;-6;24;0; L;24;6;24;-6; L;24;0;36;0;";}  //Diode
    if (comp==10) {str = "12; L;0;0;12;0; L;12;6;12;-6; L;12;6;24;0; L;12;-6;24;0; L;24;6;24;-6; L;24;0;36;0;  L;28;-6;34;-12; L;31;-12;34;-12; L;34;-12;34;-9; L;36;-6;42;-12; L;39;-12;42;-12; L;42;-12;42;-9;";}  //LED
    if (comp==11) {str = "9; L;0;0;12;0; R;12;4;36;-4; L;36;0;48;0;  L;12;-18;18;-12; L;15;-12;18;-12; L;18;-12;18;-15;  L;18;-18;24;-12; L;21;-12;24;-12; L;24;-12;24;-15;";} //LDR
    if (comp==12) {str = "8; C;8;16;36;-16; L;0;0;18;0; L;18;12;18;-12; L;18;4;40;12; L;18;-4;40;-12; L;26;4;23;10; L;23;10;35;10; L;26;4;35;10;  ";} //NPN Transistor
    if (comp==13) {str = "8; C;8;16;36;-16; L;0;0;18;0; L;18;12;18;-12; L;18;4;40;12; L;18;-4;40;-12;  L;19;-4;25;-10; L;19;-4;27;-4;  L;27;-4;25;-10;";} //PNP Transistor
	
    if (comp==14) {str = "6; L;0;0;12;0; R;12;4;36;-4; L;36;0;48;0;  L;18;12;30;-12; L;30;-12;26;-10; L;31;-12;31;-8;";} //Variable Resistor
    if (comp==15) {str = "7; L;0;0;12;0; L;12;8;12;-8; L;18;8;18;-8; L;18;0;30;0; L;6;12;24;-12;  L;24;-12;20;-10; L;25;-12;25;-8;";}  // Variable Capacitor
	
    if (comp==16) {str = "4; L;12;1;30;1; L;12;0;30;0; L;21;-9;21;9; L;22;-9;22;9;";} //Positive
    if (comp==17) {str = "2; L;12;1;30;1; L;12;0;30;0; ";} //Negative
    if (comp==18) {str = "4; L;0;0;0;12; L;-12;12;12;12; L;-8;18;8;18;  L;-4;24;4;24;";} //Ground
    if (comp==19) {str = "9; L;0;0;0;12; L;-12;12;12;12; L;-8;18;8;18;  L;-12;24;12;24; L;-8;30;8;30; L;0;30;0;42; L;4;6;10;6; L;7;3;7;9;  L;4;36;10;36;";} //Battery
    if (comp==20) {str = "4; R;0;0;12;24; L;12;24;24;36; L;12;0;24;-12; L;24;-12;24;36;";} //IC X

    if (comp==21) {str = "1; C;0;0;8;8; ";} //Connector
    if (comp==22) {str = "1; R;0;0;3;3; ";} //Dot
	
    if (comp==23) {str = "6; A;0;0;18;12;  A;0;12;18;24;  A;0;24;18;36;  A;0;36;18;48; L;0;0;12;0; L;0;48;12;48; ";} //Inductor 2 pins
    if (comp==24) {str = "7; A;0;0;18;12;  A;0;12;18;24;  A;0;24;18;36;  A;0;36;18;48;  L;0;0;12;0; L;0;48;12;48; L;0;24;12;24; ";} //Inductor 3 pins
    if (comp==25) {str = "4; A;0;0;18;12;  A;0;12;18;24;  L;0;0;12;0; L;0;24;12;24; ";} //Small inductor
    if (comp==26) {str = "11; A;0;0;18;12;  A;0;12;18;24;  A;0;24;18;36;  A;0;36;18;48; L;0;0;12;0; L;0;48;12;48;  L;24;0;24;48; C;36;0;42;8;   C;36;24;42;16;   C;36;48;42;40;  L;36;24;30;3;";} //Relay
    if (comp==27) {str = "15; A;0;0;18;12;  A;0;12;18;24;  A;0;24;18;36;  A;0;36;18;48; L;0;0;12;0; L;0;48;12;48;  L;24;0;24;48; L;30;0;30;48; ";
        str = str + " IA;36;0;54;12;  IA;36;12;54;24;  IA;36;24;54;36;  IA;36;36;54;48;  L;46;0;58;0; L;46;48;58;48; L;46;24;58;24; ";
    } //Transformer
    if (comp==28) {str = "18; A;0;0;18;12;  A;0;12;18;24;  A;0;24;18;36;  A;0;36;18;48; L;0;0;12;0; L;0;48;12;48;  L;24;0;24;48; L;30;0;30;48; ";
    str = str + " IA;36;0;54;12;  IA;36;12;54;24;  IA;36;24;54;36;  IA;36;36;54;48;  L;46;0;58;0; L;46;48;58;48; L;46;24;58;24; ";
    str = str + " L;20;52;35;-4;  L;35;-4;31;0; L;35;-4;37;0; ";
    } //RF Coil

	
    if (comp>=IC_Start) {drawIC(comp-(IC_Start-1), g, x, y, rot);} //IC X
	
    if (comp==3) {
        if (lText.Length > 0){
            Point p2 = new Point(x, y); g.DrawString(lText, dFont, dBrush, p2);
        }
    }
    if (str.Length > 0) {drawComponentX(g, str, x, y, rot);}
	
}
         

private void drawIC(int n, Graphics g, int x, int y, int rot) {
    int i, lx, ly, dx, dy, d, pin, fct, dt, x1, x2, y1, y2;
    String stn;

    //g.setFont(dFont);
    //g.setColor(dColor);//Pen and SolidBrush Color
    //g.setStroke(new BasicStroke(penSize));
    Pen pen = dPen;
	
	
    //Draw Terminals and Numbers
    dx = 12; dy = 15;
	
    d = 8;	dt = 8;
    if (n>4) dt = 16;
    if (n>4) dx = 16;
			
    pin = 1;
    lx = x; ly = y + dy;
    if (rot==0 || rot==2) {lx = x; ly = y + dy;}
    if (rot==1 || rot==3) {lx = x+dx; ly = y;}
    //n = 2;
    for (i = 0; i<n; i++ ) {//Left
        stn = Convert.ToString(pin).Replace(" ", "");pin++;
        dt = stn.Length*8;
        if (rot==0 || rot==2) {
            lx = x;
            g.DrawLine(pen, lx, ly-4, lx+d, ly-4);//Line#1
            Point p2 = new Point(lx + d + 3, ly-12); g.DrawString(stn, dFont, dBrush, p2);


            ly = ly + dy;
        }
        if (rot==1 || rot==3) {
            ly = y;
            g.DrawLine(pen, lx-4, ly, lx-4, ly+d);//Line#1
            Point p2 = new Point(lx - dt, ly + d * 2 - 12+3); g.DrawString(stn, dFont, dBrush, p2);
            lx = lx + dy;
        }
    }
	
    if (rot==0 || rot==2)	ly = ly - dy;
    if (rot==1 || rot==3)	lx = lx - dy;
	
    for (i = 0; i<n; i++ ) {//Right
        stn = Convert.ToString(pin).Replace(" ", "");pin++;
        dt = stn.Length*8;
			
            if (rot==0 || rot==2) {
                lx = x+ dx*3;
                g.DrawLine(pen, lx, ly-4, lx+d, ly-4);//Line#1
                Point p2 = new Point(lx - dt - 3, ly-12); g.DrawString(stn, dFont, dBrush, p2);
                ly = ly - dy;
            }
            if (rot==1 || rot==3) {
                ly = y+dx*3;
                g.DrawLine(pen, lx-4, ly, lx-4, ly+d);//Line#1
                Point p2 = new Point(lx - dt, ly - 12-4); g.DrawString(stn, dFont, dBrush, p2);
                lx = lx - dy;
            }
    }
	
    //Draw Rectangle
    //---------------- Rotation -----------------------------------------
    if (rot==0 || rot==2) g.DrawRectangle(pen, x+d, y, dx*3-d, dy*(n)+d);//Rectangle
    if (rot==1 || rot==3) g.DrawRectangle(pen, x, y+d, dy*(n)+d, dx*3-d);//Rectangle
}

//************************Edit Options: Copy, Cut and Paste **************************
void SelectShape(int x, int y) {
    shapeX t;
    int rx1, rx2, ry1, ry2, comp, rg;
    selectedShape = -1;
    for (int i = 0; i < compList.Count();i++) {
        
        t = compList[i];
        comp = t.shape;
        if (comp==0 || comp==1 || comp==2 || comp==5){} else {
            if (t.rot==0) {t.lpx.X = t.x + 40; t.lpx.Y = t.y + 20;}
            if (t.rot==1) {t.lpx.X = t.x + 20; t.lpx.Y = t.y + 40;}
            if (t.rot==2) {t.lpx.X = t.x - 40; t.lpx.Y = t.y - 20;}
            if (t.rot==3) {t.lpx.X = t.x - 20; t.lpx.Y = t.y - 40;}
        }
		
		
        if (t.x > t.lpx.X) {rx2 = t.x;rx1 = t.lpx.X;} else {rx1 = t.x;rx2 = t.lpx.X;}
        if (t.y > t.lpx.Y) {ry2 = t.y;ry1 = t.lpx.Y;} else {ry1 = t.y;ry2 = t.lpx.Y;}

        //drawComponent(g2, t.shape, t.x, t.y, t.rot, t.lpx, false, t.txt);
        rg = 4;
        if (x>=rx1-rg && x <=rx2+rg && y >= ry1-rg && y <= ry2+rg) {selectedShape = i;currShape = compList[i]; break;}
    }
    //selectedShape = 2;
    MoveGraphics();
    showSelectedShape();
    if (selectedShape != -1) flagSelect = false;
}
void showSelectedShape() {
    if (selectedShape != -1) {
        try {
            shapeX t;
            t = compList[selectedShape];
            dColor = Color.Blue;
            drawComponent(g1, t.shape, t.x, t.y, t.rot, t.lpx, false, t.txt);
            dColor = Color.Black;
        } catch (Exception ex) {}
    }
}
void copyShape() {
    if (selectedShape != -1) {
        currShape = compList[selectedShape];
        updateScreen();
        setEdit("Paste");
    }
}
void cutShape() {
    if (selectedShape != -1) {
        currShape = compList[selectedShape];
        compList.RemoveAt(selectedShape);
        updateScreen();
        setEdit("Paste");
    }
}
void pasteShape() {
    setEdit("");
    setShape();
}
private void setEdit(String opt) {
    flagPaste = false; flagCut = false; flagCopy = false; flagSelect = false;
    if (opt=="Cut") {
        selShape = "Cut Image";
        flagCut = true;
        flagSelect = true;
    }
    if (opt=="Copy") {
        selShape = "Copy Image";
        flagCopy = true;
        flagSelect = true;
    }
    if (opt=="Paste" && selectedShape != -1) {
        selShape = "Paste Image";
        flagPaste = true;
        flagSelect = true;
    }
    flagP1 = true;
    
    lblStatus.Text = "Selected Shape: " + selShape;
}
//*******************************List all Components**********************************

private void listAllComponents() {
	
    int x, y, i, ck, rot, fct;
    x= 25; y = 20;
    rot = rotation;
    clearScreen();
	
	
    txtInput.Text = "Text";
    for (i=0; i < cbox1.Items.Count; i++) {
        cbox1.SelectedIndex=i;
        lp1 = new Point(x+45, y+45);
            flagP1 = false;
            rotation = rot;
		
        ck = compList.Count;
        addShapeX(x, y);//Testing
        if (compList.Count > ck) {//Valid Component
            fct = 0;
            if (i>=IC_Start) {
                fct = i - (IC_Start-2);
                y = y +16*fct;
            }else {
                y = y +70;
            }
            if (y >= 440) {x = x+ 70;y = 20;}
        }
        if (i>= (IC_Start+8)) break;
		
    }
    txtInput.Text = "";
    cbox1.SelectedIndex = 0;
	
}

private void btnClear_Click(object sender, EventArgs e)
{
    clearScreen();
}

private void toolStripMenuItem3_Click(object sender, EventArgs e)
{

}

private void changeFontToolStripMenuItem_Click(object sender, EventArgs e)
{
    FontDialog fd = new FontDialog();
    if (fd.ShowDialog() == DialogResult.OK)
    {
        dFont = fd.Font;
        txtInput.Font = fd.Font;
        txtInput.Font = new Font(txtInput.Font.FontFamily, 10);
    }

}

private void changeColorToolStripMenuItem_Click(object sender, EventArgs e)
{
    ColorDialog cd = new ColorDialog();
    if (cd.ShowDialog() == DialogResult.OK)
    {
        dColor = cd.Color;
        int ps = Convert.ToInt32(penSize.Value);
        dPen = new Pen(dColor, ps);
        dBrush = new SolidBrush(dColor);
        btnColor.ForeColor = cd.Color;
    }
}

private void zoomINToolStripMenuItem_Click(object sender, EventArgs e)
{
    zoomIN();
}

private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
{
    zoomOut();
}

private void cutToolStripMenuItem_Click(object sender, EventArgs e)
{
    setEdit("Cut");
}

private void copyToolStripMenuItem_Click(object sender, EventArgs e)
{
    setEdit("Copy");
}

private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
{
    setEdit("Paste");
}

private void toolStripMenuItem4_Click(object sender, EventArgs e)
{
    SaveAsImage();
}

private void listComponentsToolStripMenuItem_Click(object sender, EventArgs e)
{
    listAllComponents();
}

private void btnRotate_Click(object sender, EventArgs e)
{
    rotation++;
    if (rotation > 3) rotation = 0; 
}

private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
{
    //About
    String msg = "";
    msg = msg + "Basic AutoCad Program \n";
    msg = msg + "Made as Part of a Training Program \n";
    msg = msg + "By Paulo Ramos @ Aug/2016 \n";
    MessageBox.Show(msg, "Basic AutoCad Program");

}
private void msgbox(String msg, String title)
{
    MessageBox.Show(msg, title);
}

        // *************************[FILES: OPEN, SAVE, SAVE AS, CLOSE]************************
                private void SaveFile()
                {
                    if (CurrFile == "" || CurrFile == null)
                    {
                        SaveAs();
                        return;
                    }
                    saveSchFile();
                }
                private void SaveAsImage()
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.InitialDirectory = "C:\\temp";
                    //sfd.DefaultExt = "Bitmaps | *.bmp | All Files | *.*";
                    sfd.FileName = "C:\\temp\\csharp1.bmp";
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        CurrFile = sfd.FileName;
                        try
                        {
                            if (currBmp != null)
                            {
                                currBmp.Save(CurrFile, System.Drawing.Imaging.ImageFormat.Bmp);
                            }
                            MessageBox.Show("File '" + CurrFile + "' Saved!", "Save File");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error Saving Bitmap to File '" + CurrFile + "\nError: " + ex.Message, "Save File Error");
                        }
                    }
                }
                private void SaveAs()
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.InitialDirectory = "C:\\temp";
                    //sfd.DefaultExt = "Bitmaps | *.bmp | All Files | *.*";
                    sfd.FileName = "C:\\temp\\csharp1.bmp";
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        currFile = sfd.FileName;
                        saveSchFile();
                    }
                }
                private void OpenFile()
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.InitialDirectory = "C:\\temp";
                    ofd.DefaultExt = "Bitmaps | *.bmp | All Files | *.*";
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        currFile = ofd.FileName;
                        openSchFile();
                    }
                    //FileStatus();
                }

        void saveSchFile() {
            String cFile = currFile;
            if (currFile=="") return;

            try {
                StreamWriter file1 = new StreamWriter(cFile);

                file1.Write("AutoCad-ProjectFile:\r\n");

                shapeX t;
                for (int i = 0; i < compList.Count;i++) {
                    t = compList[i];
                    file1.Write("Data:\r\n "+ t.shape + " " +  t.x + " " +  t.y + " " +  t.rot + " " +  t.lpx.X + " " +  t.lpx.Y + " \r\n" );
                    if (t.txt.Length>0) {
                        file1.Write("Text:\r\n "+ t.txt + " \r\n" );
                    }
                    if (t.fnt != null) {
                        file1.Write("Font:\r\n "+ t.fnt.ToString() + " \r\n" );
                    }
                }


                file1.Write("End:\r\n");
                file1.Close();

                String msg = "";
                msg = msg + "\n" + (" ********************** ");
                msg = msg + "\n" + (" ** ==File SAVED! == ** ");
                msg = msg + "\n" + (" ********************** ");
                msgbox(msg, "Basic AutoCad");
                
            } catch (IOException e) {
                msgbox("Error: " + e.Message, "Save File Error");
            }

        }


        void openSchFile() {
            if (currFile == "" || currFile == null) {
                msgbox("Select a Valid File!", "Open File Error");
                return;
            }
	
            clearScreen();
            // ****************** Opening the File ******************
            String cFile = "";
            String tmpf = "";
            cFile = currFile;
            shapeX t;
            // printf("\n%s\n", cFile);
            int shape, x, y, x2, y2, rot, ctx;
            ctx = 0;
            try {

                StreamReader file2 = new StreamReader(cFile);
                while ((tmpf = file2.ReadLine()) != null) {
			
                    if (tmpf.Equals("Data:")) {
					    String [] pr = file2.ReadLine().Split(new string[] { " " }, StringSplitOptions.None);
                        shape = Convert.ToInt32(pr[1]);
                        x = Convert.ToInt32(pr[2]);
                        y = Convert.ToInt32(pr[3]);
                        rot = Convert.ToInt32(pr[4]);
                        x2 = Convert.ToInt32(pr[5]);
                        y2 = Convert.ToInt32(pr[6]);
                        t = new shapeX(shape, x, y, rot);
                        t.lpx.X = x2;t.lpx.Y = y2;
                        compList.Add(t);
                        ctx++;
                    }
                    if (tmpf.Equals("Text:")) {
                        tmpf = file2.ReadLine();
                        compList[ctx-1].txt = tmpf;
                    }
                    if (tmpf.Equals("Font:")) {
                        tmpf = file2.ReadLine();
                        //compList[ctx-1].fnt = Font.creat
                    }
                    if (tmpf.Equals("End:")) {
                        msgbox("** End of File **", "Open File");
                        break;
                    }
                }
                file2.Close();

            } catch (IOException e) {
                msgbox("Error: " + e.Message, "Open File Error");
            }
            updateScreen();

        }


        //************************** Zoom IN and Zoom OUT *****************************************

        private void showZoom() {
            Size newSize = new Size((int)(currBmp.Width * zoom), (int)(currBmp.Height * zoom));
            Bitmap bmp = new Bitmap(currBmp, newSize);
            g1.DrawImage(bmp, 0, 0);
        }
    
        private void zoomIN() {
            zoom += 0.25f;
            flagZoom = true;
            showZoom();
        }
        private void zoomOut() {
            zoom = 1.00f;
            flagZoom = false;
			
        }
        //*/
        //*************************************************************************************



    }//End of Class
    
}//End of NameSpace

class shapeX
{
    public int shape = 0, x = 0, y = 0, rot = 0;
    public Point lpx = new Point(0, 0);
    public String txt = "";
    public Font fnt = null;

    public shapeX(int shape1, int x1, int y1, int rot)
    {
        this.shape = shape1;
        this.x = x1;
        this.y = y1;
        this.rot = rot;
    }
}
