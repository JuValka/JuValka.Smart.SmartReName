using Microsoft.VisualBasic;
using System.Text;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using SmartTools.FileSys;
using SmartTools.About;
using SmartTools.User;
using SmartTools.Version;
using SmartTools.Settings;
using SmartTools.OPENFILE.API;
using SmartTools.ArrayRN;
namespace SmartTools.PRN
{
    public partial class SmartRN : Form
    {
        public SmartRN()
        {
            InitializeComponent();
        }
       // OPENFILES ofs = new OPENFILES();

        private readonly bool yes = true, no = false;

        private void Readme_Click(object sender, EventArgs e)
        {
            Aboutme();
            //Process.Start("https://bbs.iassist.top/forum.php?mod=viewthread&tid=1619&extra=page%3D1");
        }

        /// <summary>
        /// 选择的文件
        /// </summary>
        public static string[] loadfiles;


        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SmartRnDlgOPEN_Click(object sender, EventArgs e)
        {
            LoadFilesdlg();
            ListDatas();
        }

        /// <summary>
        /// 读取列表列信息
        /// </summary>
        private void ListDatas()
        {
            Thread th = new Thread(new ThreadStart(LoadDataList)) { IsBackground = yes };
            th.Start();
        }

        /// <summary>
        /// 加载列表值
        /// </summary>
        private void LoadDataList()
        {
            try
            {
                PGB.Value = 0;
                PGB.Maximum = loadfiles.Length;
                PGB.Step = (PGB.Maximum / 100);
                DataList.BeginUpdate();
                for (int i = 0; i < loadfiles.Length; i++)//添加列表项,,,+1
                {
                    ListViewItem lvi = new ListViewItem
                    {
                        Text = (1 + i).ToString()//id                              ///第一列
                    };//创建项                                                     
                    lvi.SubItems.Add(Path.GetFileName(loadfiles[i]));//名称        ///第二列
                    lvi.SubItems.Add(loadfiles[i]);//路径                          ///第三列        
                    lvi.SubItems.Add(Path.GetFileName(loadfiles[i]));//预览        ///第四列    
                    lvi.SubItems.Add(Path.GetExtension(loadfiles[i]));//扩展名  
                    lvi.SubItems.Add("◎");
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add(File.GetCreationTime(loadfiles[i]).ToString());
                    DataList.Items.Add(lvi);
                    ///添加进度条导致执行变慢,用进度天百分比模式减少进度条刷新率
                    try
                    {
                        if (i % PGB.Step == 0)//当循环i次 除以1%次 刚好余0时刷新进度条
                        {
                            PGB.Value += i;
                        }
                    }
                    catch { PGB.Value -= (PGB.Value - PGB.Maximum); }//因为最后+i后的value可能大于maxnmum,所以减去超过最大限制的值               
                }
                DataList.EndUpdate();//结束刷新         
                PGB.Value = 0;
                LBFileValue.Text = (DataList.Items.Count).ToString();
                YuLan();//刷新第四列
                MenuEnable();
                offsetext.Maximum = Path.GetFileNameWithoutExtension(DataList.Items[0].SubItems[2].Text).Length;
              
            }
            catch { }
        }

        /// <summary>
        /// 选择文件
        /// </summary>
        private void LoadFilesdlg()
        {
            try
            {
                loadfiles = OPENFILES.Opens("添加文件", "所有文件|*.*|bxyz|*.bxyz|JPG|*.jpg|PNG|*.png", true);
            }
            catch { }
        }

        /// <summary>
        /// 列表右键菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RKeyMenuAddFile_Click(object sender, EventArgs e)
        {
            LoadFilesdlg();
            ListDatas();
        }

        /// <summary>
        /// 清空列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DataList.Items.Clear();
                loadfiles = null;
                LBFileValue.Text = DataList.Items.Count.ToString();
                MenuEnable();
                offsetext.Maximum = 0;
            }
            catch { }
        }

        /// <summary>
        /// 移除列表选定项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveSelectItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (ListViewItem lvi in DataList.SelectedItems)
                {
                    DataList.Items.RemoveAt(lvi.Index);
                }
                LBFileValue.Text = DataList.Items.Count.ToString();
                MenuEnable();
                offsetext.Maximum = Path.GetFileNameWithoutExtension(DataList.Items[0].SubItems[2].Text).Length;
                if (NameIndex.TextLength != 0)
                {
                    NameAdd_0_Length();
                }
            }
            catch { }
        }

        /// <summary>
        /// 选择全部
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < DataList.Items.Count; i++)
                {
                    DataList.Items[i].Selected = yes;
                }
            }
            catch { }
        }

        /// <summary>
        /// 前缀内容输入时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TexHead_TextChanged(object sender, EventArgs e)
        {
            YuLan();
        }

        /// <summary>
        /// YuLan预览
        /// </summary>
        private void YuLan()
        {
            try
            {
                if (addStr.Checked == yes)
                {
                    //添加字符
                    ADD_STR();
                }
                if (delStr.Checked == yes)//删除指定位置字符的长度
                {
                    if (insertValue.Text.Length == 0)//还原
                    {
                        TexHeadAndHou();
                    }
                    //删除字符
                    Remove_STR();
                }

                //t替换指定字符串
                if (ReplaceStr.TextLength > 0)
                {
                    Replace_STR();
                }

                //删除指定字符串
                if (DelstrsValue.TextLength > 0)
                {
                    DEL_STR();
                }
                //流水号命名
                if (NameIndex.Text != "")
                {
                    NAME_ID_STEP();
                }
                //列表宽度自适应
                // ViewEye.Width = DataList.Items[0].SubItems[3].Text.Length * 10;

                //清理数组
                //loadfiles = null;
            }
            catch { }
        }
        /// <summary>
        /// 调用文本的预览 
        /// </summary>
        /// <param name="readloadtxt">文本中的项</param>
        private void YuLan(string[] readloadtxt)
        {
            if (readloadtxt != null)
            {
                try
                {
                    for (int i = 0; i < DataList.Items.Count; i++)
                    {
                        DataList.Items[i].SubItems[3].Text = readloadtxt[i];
                    }
                    readloadtxt = null;
                }
                catch { }
            }
        }
        /// <summary>
        /// 删除指定字符串
        /// </summary>
        private void DEL_STR()
        {
            for (int i = 0; i < DataList.Items.Count; i++)
            {
                DataList.Items[i].SubItems[3].Text = DataList.Items[i].SubItems[3].Text.Replace(DelstrsValue.Text, "");//用重写Replace替换新字符串
            }
        }

        private void Replace_STR()
        {
            for (int i = 0; i < DataList.Items.Count; i++)
            {
                DataList.Items[i].SubItems[3].Text = DataList.Items[i].SubItems[3].Text.Replace(ReplaceStr.Text, ReplaceStred.Text);//用重写Replace替换新字符串
            }
        }

        /// <summary>
        /// 移除内容
        /// </summary>
        private void Remove_STR()
        {
            try
            {
                for (int i = 0; i < DataList.Items.Count; i++)//第三列赋值
                {
                    if (RExpCheck.Checked == no)
                    {
                        DataList.Items[i].SubItems[3].Text
                            = TexHead.Text
                            + Path.GetFileNameWithoutExtension(DataList.Items[i].SubItems[2].Text).Remove(Convert.ToInt32(offsetext.Value), Convert.ToInt32(insertValue.Text))
                            + TextFoot.Text
                            + DataList.Items[i].SubItems[4].Text;
                    }
                    else
                    {
                        DataList.Items[i].SubItems[3].Text
                            = TexHead.Text
                            + Path.GetFileNameWithoutExtension(DataList.Items[i].SubItems[2].Text).Remove(Convert.ToInt32(offsetext.Value), Convert.ToInt32(insertValue.Text))
                            + TextFoot.Text
                            + "."
                            + ExpValue.Text;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// 插入内容
        /// </summary>
        private void ADD_STR()
        {
            try
            {
                for (int i = 0; i < DataList.Items.Count; i++)//DataList.Items.Count数据行数//给第三列赋值
                {
                    if (RExpCheck.Checked == no)
                    {
                        //修改第3列表格文本
                        DataList.Items[i].SubItems[3].Text
                            = TexHead.Text
                            + Path.GetFileNameWithoutExtension(DataList.Items[i].SubItems[2].Text).Insert(Convert.ToInt32(offsetext.Value), insertValue.Text)
                            + TextFoot.Text
                            + DataList.Items[i].SubItems[4].Text;
                    }
                    else
                    {
                        DataList.Items[i].SubItems[3].Text
                            = TexHead.Text
                            + Path.GetFileNameWithoutExtension(DataList.Items[i].SubItems[2].Text).Insert(Convert.ToInt32(offsetext.Value), insertValue.Text)
                            + TextFoot.Text
                            + "."
                            + ExpValue.Text;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// 当后缀内容输入时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextFoot_TextChanged(object sender, EventArgs e)
        {
            YuLan();
        }

        /// <summary>
        /// 只有前后缀
        /// </summary>
        private void TexHeadAndHou()
        {
            try
            {
                for (int i = 0; i < DataList.Items.Count; i++)//给第三列赋值
                {
                    if (RExpCheck.Checked == no)
                    {
                        DataList.Items[i].SubItems[3].Text
                            = TexHead.Text
                            + Path.GetFileNameWithoutExtension(DataList.Items[i].SubItems[2].Text)
                            + DataList.Items[i].SubItems[4].Text;
                    }
                    else
                    {
                        DataList.Items[i].SubItems[3].Text
                            = TexHead.Text
                            + Path.GetFileNameWithoutExtension(DataList.Items[i].SubItems[2].Text)
                            + TextFoot.Text
                            + "."
                            + ExpValue.Text;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// 插入内容输入时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InsertValue_TextChanged(object sender, EventArgs e)
        {
            YuLan();
        }

        /// <summary>
        /// "添加" 按钮选中后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddStr_CheckedChanged(object sender, EventArgs e)
        {
            YuLan();
        }

        /// <summary>
        /// 偏移的位置更改时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Offsetext_ValueChanged(object sender, EventArgs e)
        {
            if (insertValue.Text != "")
            {
                YuLan();
            }
        }

        /// <summary>
        /// "删除"按钮选中时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DelStr_CheckedChanged(object sender, EventArgs e)
        {
            YuLan();
        }

        /// <summary>
        /// 扩展名启用时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RExpCheck_CheckedChanged(object sender, EventArgs e)
        {
            ExpValue.Enabled = RExpCheck.Checked == yes ? yes : no;//后缀是否选中,文本框是否可用
            YuLan();
        }

        /// <summary>
        /// 扩展名修改时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExpValue_TextChanged(object sender, EventArgs e)
        {
            YuLan();
        }

        /// <summary>
        /// 应用改名按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppRUN_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(new ThreadStart(GoReName)) { IsBackground = yes }.Start();
                MENU_Utools.Enabled = yes;
                tsUtools.Enabled = yes;
            }
            catch { }
        }

        /// <summary>
        /// 改名方法
        /// </summary>
        private void GoReName()
        {
            try
            {
                PGB.Value = 0;
                PGB.Maximum = DataList.Items.Count;
                PGB.Step = PGB.Maximum / 100;
                for (int i = 0; i < DataList.Items.Count; i++)
                {
                    try
                    {
                        FileSystem.Rename(DataList.Items[i].SubItems[2].Text, Path.GetDirectoryName(DataList.Items[i].SubItems[2].Text) + "\\" + DataList.Items[i].SubItems[3].Text);
                        DataList.Items[i].SubItems[5].Text = "✔";
                        DataList.Items[i].SubItems[6].Text = DataList.Items[i].SubItems[1].Text;
                        DataList.Items[i].SubItems[1].Text = DataList.Items[i].SubItems[3].Text;
                        DataList.Items[i].SubItems[2].Text = Path.GetDirectoryName(DataList.Items[i].SubItems[2].Text) + "\\" + DataList.Items[i].SubItems[1].Text;

                        try
                        {
                            if (i % PGB.Step == 0)//当循环i次 除以 1%次 刚好余0时刷新进度条
                            {
                                PGB.Value += PGB.Step;
                            }
                        }
                        catch { PGB.Value -= (PGB.Value - PGB.Maximum); }
                    }
                    catch { DataList.Items[i].SubItems[5].Text = "✖"; }
                }
            }
            catch { }
        }

        /// <summary>
        /// 气泡提示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InsertValue_Click(object sender, EventArgs e)
        {
            try
            {
                ToolTip toolTip = new ToolTip
                {
                    ReshowDelay = 500
                };
                ToolTip tp = toolTip;
                tp.ToolTipTitle = "提示:";
                tp.SetToolTip(insertValue, "输入要添加的字符或要删除的字符个数");
                tp.Active = yes;
            }
            catch { }
        }

        /// <summary>
        /// 删除的字符串更改时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DelstrsValue_TextChanged(object sender, EventArgs e)
        {
            YuLan();
        }

        private void 调用文本重命名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            USE_TEXT_RENAME();
        }
        private void USE_TEXT_RENAME()
        {
            try
            {
                //using (OpenFileDialog ofd = new OpenFileDialog())
                //{
                //    ofd.Filter = "文本文档|*.txt|所有文件|*.*";
                //    ofd.Title = "打开命名文件";
                //    ofd.ShowDialog();
                    
                //}
                YuLan(File.ReadAllLines(OPENFILES.Open("打开命名文件", "文本文档|*.txt|所有文件|*.*")));
            }
            catch { }
        }
        private void UpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UPDATE_VERSION();
        }

        private void 撤消UToolStripMenuItem_Click(object sender, EventArgs e)
        {

            tsUAction();
        }

        private void tsUAction()
        {
            try
            {
                for (int i = 0; i < DataList.Items.Count; i++)
                {
                    DataList.Items[i].SubItems[3].Text = DataList.Items[i].SubItems[6].Text;
                }
                new Thread(new ThreadStart(GoReName)) { IsBackground = yes }.Start();
                MENU_RTools.Enabled = yes;
                tsRtools.Enabled = yes;
            }
            catch { }
        }


        /// <summary>
        /// 窗体加载时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SmartRN_Dlg_Load(object sender, EventArgs e)
        {
            MenuEnable();
            this.DataList.ListViewItemSorter = new ListViewColumnSorter();
            this.DataList.ColumnClick += new ColumnClickEventHandler(ListViewHelper.ListView_ColumnClick);

        }

        /// <summary>
        /// 恢复命名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RTools_Click(object sender, EventArgs e)
        {
            tsUAction();
        }
        /// <summary>
        /// 取消全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnselectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < DataList.Items.Count; i++)
                {
                    DataList.Items[i].Selected = no;
                }
            }
            catch { }
        }
        string[] str_Drop;
        /// <summary>
        /// 拖放数据到控件时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataList_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Link;
            str_Drop = (String[])e.Data.GetData(DataFormats.FileDrop, true);//检索数据格式相关联的数据  
        }

        /// <summary>
        /// 拖拽加载数据的方法
        /// </summary>
        /// <param name="str_">object 类型传值给str_darp</param>
        private void LoadDataList(object str_)
        {
            loadfiles = null;
            loadfiles = str_ as string[];
            PGB.Value = 0;
            PGB.Maximum = loadfiles.Length;
            PGB.Step = (PGB.Maximum / 100);
            DataList.BeginUpdate();
            for (int i = 0; i < loadfiles.Length; i++)
            {
                ListViewItem lvi = new ListViewItem
                {
                    Text = (1 + DataList.Items.Count).ToString()//id                              ///第一列
                };//创建项                                                     
                lvi.SubItems.Add(Path.GetFileName(loadfiles[i]));//名称        ///第二列
                lvi.SubItems.Add(loadfiles[i]);//路径                          ///第三列        
                lvi.SubItems.Add(Path.GetFileName(loadfiles[i]));//预览        ///第四列    
                lvi.SubItems.Add(Path.GetExtension(loadfiles[i]));//扩展名  
                lvi.SubItems.Add("◎");
                lvi.SubItems.Add("");
                DataList.Items.Add(lvi);
                ///添加进度条导致执行变慢,用进度天百分比模式减少进度条刷新率
                try
                {
                    if (i % PGB.Step == 0)//当循环i次 除以1%次 刚好余0时刷新进度条
                    {
                        PGB.Value += i;
                    }
                }
                catch { PGB.Value -= (PGB.Value - PGB.Maximum); }//因为最后+i后的value可能大于maxnmum,所以减去超过最大限制的值               
            }
            DataList.EndUpdate();//结束刷新         
            PGB.Value = 0;
            LBFileValue.Text = (DataList.Items.Count).ToString();
            YuLan();//刷新第四列
            MenuEnable();
        }

        /// <summary>
        /// 启用的功能
        /// </summary>
        private void MenuEnable()
        {
            addStr.Checked = yes;
            Control.CheckForIllegalCrossThreadCalls = no;
            ExpValue.Enabled = no;
            RExpCheck.Checked = no;
            SelectNameModel.Text = SelectNameModel.Items[0].ToString();
            offsetext.Maximum = 0;
            tsView.Enabled = no;
            if (DataList.Items.Count == 0)
            {
                MENU_Utools.Enabled = no;
                MENU_RTools.Enabled = no;
                MENUAppRUN.Enabled = no;
                MENUSelectTxtRen.Enabled = no;
                MENU_RTools.Enabled = no;
                tsApply.Enabled = no;
                MENU_Utools.Enabled = no;
                MENU_RTools.Enabled = no;
                tsRtools.Enabled = no;
                tsUtools.Enabled = no;
                tsRemove.Enabled = no;
                selectAllToolStripMenuItem.Enabled = no;
                unselectAllToolStripMenuItem.Enabled = no;
                clearToolStripMenuItem.Enabled = no;
                removeSelectItemToolStripMenuItem.Enabled = no;
                tsChangeFileTime.Enabled = no;
                tsRNFOR_TEXT.Enabled = no;
                MENUChangeFileTIme.Enabled = no;
                MENU_tsSave.Enabled = no;
                SmartRnDlgSAVE.Enabled = no;
            }
            else
            {
                tsRemove.Enabled = yes;
                MENUAppRUN.Enabled = yes;
                MENUSelectTxtRen.Enabled = yes;
                tsApply.Enabled = yes;
                selectAllToolStripMenuItem.Enabled = yes;
                tsView.Enabled = yes;
                clearToolStripMenuItem.Enabled = yes;
                tsChangeFileTime.Enabled = yes;
                tsRNFOR_TEXT.Enabled = yes;
                MENUChangeFileTIme.Enabled = yes;
                MENU_tsSave.Enabled = yes;
                SmartRnDlgSAVE.Enabled = yes;
            }
        }

        private void SmartRnDlgEXIT_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ToolStripOpen_Click(object sender, EventArgs e)
        {
            LoadFilesdlg();
            ListDatas();
        }

        private void ToolStripRemove_Click(object sender, EventArgs e)
        {
            try
            {
                DataList.Items.Clear();
                loadfiles = null;
                LBFileValue.Text = DataList.Items.Count.ToString();
                MenuEnable();
                offsetext.Maximum = 0;
            }
            catch { }
        }

        private void ToolStripApply_Click(object sender, EventArgs e)
        {
            try
            {
                PGB.Value = 0;
                PGB.Maximum = DataList.Items.Count;
                PGB.Step = PGB.Maximum / 100;
                new Thread(new ThreadStart(GoReName)) { IsBackground = yes }.Start();
            }
            catch { }
        }

        private void NameIndex_TextChanged(object sender, EventArgs e)
        {
            if (NameIndex.TextLength != 0 && NameNumCheck.Checked == false && NameIndexNum.Text != "")
            {
                YuLan();
            }
            else if (NameNumCheck.Checked == true && NameIndexNum.Text != "")
            {
                NameAdd_0_Length();
            }
        }

        /// <summary>
        /// 流水号命名
        /// </summary>
        private void NAME_ID_STEP()
        {
            try
            {
                if (NameIndexNum.Text == "")
                {
                    return;
                }
                int name_index = Convert.ToInt32(NameIndexNum.Text);
                int step = (int)NameStepNum.Value;
                string name = NameIndex.Text;
                if (NameNumCheck.Checked == true)
                {
                    name_index -= 1;
                    int x = (int)NameStepAdd0.Value;//文件个数值的位数
                    string NameNumStart = "";
                    for (int i = 0; i < DataList.Items.Count; i++)
                    {
                        switch (x)
                        {
                            case 1: NameNumStart = (step + name_index).ToString("0"); break;
                            case 2: NameNumStart = (step + name_index).ToString("00"); break;
                            case 3: NameNumStart = (step + name_index).ToString("000"); break;
                            case 4: NameNumStart = (step + name_index).ToString("0000"); break;
                            case 5: NameNumStart = (step + name_index).ToString("00000"); break;
                            case 6: NameNumStart = (step + name_index).ToString("000000"); break;
                            case 7: NameNumStart = (step + name_index).ToString("0000000"); break;
                            case 8: NameNumStart = (step + name_index).ToString("00000000"); break;
                        }

                        if (name.Contains("#") || name.Contains("Name"))
                        {
                            DataList.Items[i].SubItems[3].Text = name.Replace("#", NameNumStart).Replace("Name", Path.GetFileNameWithoutExtension(DataList.Items[i].SubItems[1].Text)) + DataList.Items[i].SubItems[4].Text;
                        }
                        else
                        {
                            DataList.Items[i].SubItems[3].Text = name + NameNumStart + DataList.Items[i].SubItems[4].Text;
                        }
                        name_index += step;
                    }
                }
                else
                {
                    for (int i = 0; i < DataList.Items.Count; i++)
                    {
                        if (name.Contains("#") || name.Contains("Name"))
                        {
                            DataList.Items[i].SubItems[3].Text = name.Replace("#", name_index.ToString()).Replace("Name", Path.GetFileNameWithoutExtension(DataList.Items[i].SubItems[1].Text)) + DataList.Items[i].SubItems[4].Text;
                        }
                        else
                        {
                            DataList.Items[i].SubItems[3].Text = name + name_index.ToString() + DataList.Items[i].SubItems[4].Text;
                        }
                        name_index += step;
                    }
                }
            }
            catch { }
        }

        private void NameNumCheck_CheckedChanged(object sender, EventArgs e)
        {
            NameAdd_0_Length();
        }

        /// <summary>
        /// 识别最大补齐位数
        /// </summary>
        private void NameAdd_0_Length()
        {
            try
            {
                if (NameIndex.TextLength != 0)
                {
                    if ((Convert.ToInt32(NameIndexNum.Text) + DataList.Items.Count).ToString().Length >= DataList.Items.Count.ToString().Length)
                    {
                        NameStepAdd0.Minimum = (Convert.ToInt32(NameIndexNum.Text) + DataList.Items.Count).ToString().Length;
                        NameStepAdd0.Value = NameStepAdd0.Minimum;
                    }
                    YuLan();
                }
            }
            catch { }
        }

        private void NameIndexNum_TextChanged(object sender, EventArgs e)
        {
            if (NameIndex.TextLength != 0 && NameNumCheck.Checked == false && NameIndexNum.Text != "")
            {
                YuLan();
            }
            else if (NameNumCheck.Checked == true && NameIndexNum.Text != "")
            {
                NameAdd_0_Length();
            }
        }

        private void NameStepNum_ValueChanged(object sender, EventArgs e)
        {
            YuLan();
        }

        /// <summary>
        /// 递增值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Setp_add__ValueChanged(object sender, EventArgs e)
        {
            if (NameNumCheck.Checked == true)
            {
                NameAdd_0_Length();
            }
        }

        /// <summary>
        /// 模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectNameModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectNameModel.Text = SelectNameModel.SelectedItem.ToString();
            if (SelectNameModel.Text != "选择模板" && SelectNameModel.Text != "清除模板")
            {
                NameIndex.Text = SelectNameModel.Text;
            }
            else
            {
                NameIndex.Text = "";
            }
        }

        private void ToolStripButton2_Click(object sender, EventArgs e)
        {

            if (tsTop.Checked)
            {
                tsTop.Checked = false;
                TopMost = false;
            }
            else
            {
                TopMost = true;
                tsTop.Checked = true;
            }
        }

        /// <summary>
        /// 替换字符串前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReplaceStr_TextChanged(object sender, EventArgs e)
        {
            YuLan();
        }

        /// <summary>
        /// 替换字符串后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReplaceStred_TextChanged(object sender, EventArgs e)
        {
            YuLan();
        }

        /// <summary>
        /// 点击列表头时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataList_ColumnClick(object sender, ColumnClickEventArgs e)
        {

        }

        /// <summary>
        /// 撤销
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsUtools_Click(object sender, EventArgs e)
        {
            tsUAction();
        }

        /// <summary>
        /// 列表项选定状态更改时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                removeSelectItemToolStripMenuItem.Enabled = yes;
                unselectAllToolStripMenuItem.Enabled = yes;
            }
            else
            {
                removeSelectItemToolStripMenuItem.Enabled = no;
                unselectAllToolStripMenuItem.Enabled = no;
            }
        }

        private void tsRtools_Click(object sender, EventArgs e)
        {
            tsUAction();
        }

        /// <summary>
        /// 拖放完成时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataList_DragDrop(object sender, DragEventArgs e)
        {
            Thread th = new Thread(new ParameterizedThreadStart(LoadDataList)) { IsBackground = yes };
            th.Start(str_Drop);
        }

        private void SettingsItem_Click(object sender, EventArgs e)
        {
            ShowSettings();
        }

        private void toolStripSettings_Click(object sender, EventArgs e)
        {
            ShowSettings();
        }

        private void ShowSettings()
        {
            using (RNSettings set = new RNSettings())
            {
                set.ShowInTaskbar = false;
                set.ShowDialog();
            }
        }

        private void toolStripView_Click(object sender, EventArgs e)
        {
            YuLan();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            UPDATE_VERSION();
        }

        private void UPDATE_VERSION()
        {
            using (FindVersion fv = new FindVersion())
            {
                try
                {
                    fv.SetXml(@"https://api.iassist.top/tools/appUpdate/Update.xml");
                    string lv = fv.LocalVersion();
                    string rv = fv.RemoteVersion();
                    if (fv.IsDownVersion(rv, lv))
                    {
                        DialogResult dr = MessageBox.Show("检测到新版本");
                        if (dr == DialogResult.OK || dr == DialogResult.None)
                        {
                            fv.ShowDialog();
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("无网络连接");
                }
            }
        }

        private void tsRNFOR_TEXT_Click(object sender, EventArgs e)
        {
            USE_TEXT_RENAME();
        }

        private void tsChangeFileTime_Click(object sender, EventArgs e)
        {
            using (RNChangeTime ct = new RNChangeTime())
            {
                ct.ShowInTaskbar = false;
                ct.ShowDialog();
            }
        }

        private void MENUChangeFileTIme_Click(object sender, EventArgs e)
        {
            using (RNChangeTime ct = new RNChangeTime())
            {
                ct.ShowInTaskbar = false;
                ct.ShowDialog();
            }
        }

        private void tsAbout_Click(object sender, EventArgs e)
        {
            Aboutme();
        }
        private void Aboutme()
        {
            using (RNAbout ab = new RNAbout())
            {
                ab.ShowDialog();
            }
        }

    /*    private string SetSavePath(string SaveFilter)
        {
            using (SaveFileDialog sf = new SaveFileDialog())
            {
                sf.Filter = SaveFilter;
                sf.Title = " 保存文件";
                sf.OverwritePrompt = yes;
                sf.ShowDialog();
                return sf.FileName;
            }
        }
       */

        private void MENU_tsSave_Click(object sender, EventArgs e)
        {
            string Save = OPENFILES.Save(" 保存文件", "VK|*.vk|文件|*.*");
            byte[] buf;
            try
            {
                using (FileStream fs = new FileStream(Save, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                {
                    for (int i = 0; i < DataList.Columns.Count; i++)
                    {
                        buf = Encoding.Default.GetBytes(DataList.Columns[i].Name);
                        fs.Write(buf, 0, buf.Length);
                    }

                    for (int i = 0; i < DataList.Items.Count; i++)
                    {
                        for (int j = 0; j < DataList.Columns.Count; j++)
                        {
                            buf = Encoding.Default.GetBytes(DataList.Items[i].SubItems[j].Text + "\t\r\n");
                            fs.Write(buf, 0, buf.Length);
                        }
                    }
                    fs.Close();
                }
            }
            catch { }
        }

        private void sign_Click(object sender, EventArgs e)
        {
            using (UserLogin Login = new UserLogin())
            {
                Login.ShowDialog();
            }
        }

        private void SmartRnDlgSAVE_Click(object sender, EventArgs e)
        {
            OPENFILES.Save("保存文件", "VK|*.vk|文件|*.*");
        }
    }
}
