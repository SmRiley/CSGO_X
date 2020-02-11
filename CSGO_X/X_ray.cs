using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;

namespace CSGO_X
{
    struct Obj_Struct{
        public int Self_ADDR;
        public int ID;
        public int Pre, Next;
    }

    struct Person_Struct {
        public int Health;
        //CT(警)3,T(匪)2
        public int army;
        public float X, Y, Z;
    }

    struct ViewScreen {
        public float X, Y;
       /* public ViewScreen(float x,float y) {
            X = x;
            Y = y;
        }*/
    }


    class X_Ray
    {
        
        static int DllPtr;
        int self_offset = Convert.ToInt32("4C41704", 16);
        int health_offset = Convert.ToInt32("FC", 16);
        int army_offset = Convert.ToInt32("F0", 16);
        int ViewMatrix_offset = Convert.ToInt32("4C33134", 16);
        int X_offset = Convert.ToInt32("134", 16);
        int Y_offset = Convert.ToInt32("138", 16);
        int Z_offset = Convert.ToInt32("13C", 16);
        public List<Person_Struct> Person_List = new List<Person_Struct>();
        Timer Scan_Timer;
        bool Timer_State = false;
        Memory Memory_Manage;
        public X_Ray(string PName) {
            Memory_Manage = new Memory(Form1.PName);
            DllPtr = Memory_Manage.GetModuleHandle(Form1.ModuleName).ToInt32();
            var Self_Struct = Init_Struct(DllPtr + self_offset);
            //得到链表第一项
            var First_Node = Get_First_Node(Self_Struct);
            Scan_Timer = new Timer(Timer_Call, First_Node, 5, 10);
        }

        void  Timer_Call(object state) {
            if (!Timer_State)
            {
                Timer_State = true;
                var Rs_List = new List<Person_Struct>();
                var Index_Node = (Obj_Struct)state;               
                var self = Init_Person(Init_Struct(DllPtr + self_offset).Self_ADDR);
                while (Index_Node.Next != 0)
                {
                    Index_Node = Init_Struct(Index_Node.Next);
                    var To_Person = Init_Person(Index_Node.Self_ADDR);
                    // && 
                    if (To_Person.Health > 0 && !new float[] { To_Person.X, To_Person.Y, To_Person.Z }.Contains(0))
                    {
                        if (To_Person.X != self.X) {
                            Rs_List.Add(To_Person);
                        }
                        //Console.WriteLine(To_Person.Health);
                     
                    }
                }
                Draw_Rectangle(self,Rs_List);
                Person_List = Rs_List;
                Timer_State = false;
            }          
        }

        Obj_Struct Init_Struct(int BaseADDR) {
            //Console.WriteLine(BaseADDR.ToString("X"));
            Obj_Struct Rs;
            Rs.Self_ADDR = Memory_Manage.Read_MemoryInt32(BaseADDR);
            Rs.ID = Memory_Manage.Read_MemoryInt32(BaseADDR + 4);
            Rs.Pre = Memory_Manage.Read_MemoryInt32(BaseADDR + 8);
            Rs.Next = Memory_Manage.Read_MemoryInt32(BaseADDR + 12);
            return Rs;
        }

        Obj_Struct Get_First_Node(Obj_Struct Any_Node) {
            var First_Node = Any_Node;
            while (First_Node.Pre != 0) {
                First_Node = Init_Struct(First_Node.Pre);
            }
            return First_Node;
        }

        Person_Struct Init_Person(int BaseADDR) {
            Person_Struct Ps;
            Ps.Health = Memory_Manage.Read_MemoryInt32(BaseADDR + health_offset);
            Ps.army = Memory_Manage.Read_MemoryInt32(BaseADDR + army_offset);
            Ps.X = Memory_Manage.Read_MemoryFloat(BaseADDR + X_offset);
            Ps.Y = Memory_Manage.Read_MemoryFloat(BaseADDR + Y_offset);
            Ps.Z = Memory_Manage.Read_MemoryFloat(BaseADDR + Z_offset);
            return Ps;
        }

        /// <summary>
        /// 得到视角矩阵
        /// </summary>
        /// <param name="ViewADDR">视角首地址</param>
        /// <returns></returns>
        float[,] Get_ViewMatrix(int ViewADDR) {
            var Rs = new float[4,4];
            for (int I = 0; I < Rs.GetLength(0); I++)
            {
                for (int i = 0; i < Rs.GetLength(1); i++)
                {
                    Rs[I, i] = Memory_Manage.Read_MemoryFloat(ViewADDR+(i+(I*4))*4);
                }
            }
            return Rs;
        }

        ViewScreen WorldToScreen(Person_Struct Ps,float[,] ViewMatrix) {
            var Sigma_Matrix = new float[4];
            for (int i = 0; i < Sigma_Matrix.Length; i++)
            {
                Sigma_Matrix[i] = ViewMatrix[i, 0] * Ps.X + ViewMatrix[i, 1] * Ps.Y + ViewMatrix[i, 2] * Ps.Z + ViewMatrix[i, 3];
            }
            if (Sigma_Matrix[3] < 0.01) {
                return new ViewScreen() {X = 0,Y=0};
            }
            float X = Sigma_Matrix[0] / Sigma_Matrix[3];
            float Y = Sigma_Matrix[1] / Sigma_Matrix[3];
            float Z = Sigma_Matrix[2] / Sigma_Matrix[3];
            float Window_H = 1080 / 2;//屏高
            float Window_W = 1920 / 2;//屏框
            ViewScreen Rs;
            Rs.X = X * Window_W + Window_W + X;
            Rs.Y = -(Y * Window_H) + Window_H + Y;
            return Rs;
        }

        void Draw_Rectangle(Person_Struct self,List<Person_Struct> Other_list) {
            //求距离
            try
            {
                var Rectangles = new List<(Rectangle,Color)>();
                foreach (var Other in Other_list)
                {
                    var VM = WorldToScreen(Other, Get_ViewMatrix(DllPtr + ViewMatrix_offset));
                    double M = Math.Sqrt(Math.Pow((double)(self.X - Other.X), 2) + Math.Pow((double)(self.Y - Other.Y), 2) + Math.Pow((double)(self.Z - Other.Z), 2)) / 30;
                    int H = Convert.ToInt32(950 / M * 2);
                    int W = Convert.ToInt32(400 / M * 2);
                    if (Other.army == self.army)
                    {
                        Rectangles.Add((new Rectangle((int)VM.X - W / 2, (int)VM.Y - H, W, H), Color.Coral));
                    }
                    else {
                        Rectangles.Add((new Rectangle((int)VM.X - W / 2, (int)VM.Y - H, W, H), Color.Red));
                    }                   
                }
                Form1.bitmap_form.Drawing(Rectangles);
                               
            }
            catch { 
            
            }
        }
    }
}
