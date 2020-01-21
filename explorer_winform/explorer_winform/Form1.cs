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

namespace explorer_winform
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            //보기 옵션 콤보 설정
            cboListViewMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboListViewMode.Items.Add("Details");
            cboListViewMode.Items.Add("SmallIcon");
            cboListViewMode.Items.Add("LargeIcon");
            cboListViewMode.Items.Add("List");
            cboListViewMode.Items.Add("Tile");
            cboListViewMode.SelectedIndex = 0;
        }

        private void form1_load(object sender, EventArgs e)
        {
            // 현재 사용자 정보 표시
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            label1.Text = "현재 사용자: " + identity.Name;

            // 현재 로컬 컴퓨터에 존재하는 드라이브 정보 검색, 트리노드에 추가
            DriveInfo[] all_drives = DriveInfo.GetDrives();
            foreach( DriveInfo dname in all_drives )
            {
                if( dname.DriveType == DriveType.Fixed )
                {
                    if( dname.Name == @"C:\" )
                    {
                        TreeNode root_node = new TreeNode(dname.Name);
                        root_node.ImageIndex = 0;
                        root_node.SelectedImageIndex = 0;
                        treeView1.Nodes.Add(root_node);
                        Fill(root_node);
                    }
                    else
                    {
                        TreeNode root_node = new TreeNode(dname.Name);
                        root_node.ImageIndex = 1;
                        root_node.SelectedImageIndex = 1;
                        treeView1.Nodes.Add(root_node);
                        Fill(root_node);
                    }
                }
            }

            // 첫번째 노드 확장
            treeView1.Nodes[0].Expand();

            // list view 보기 속성 설정
            //listView1.View = View.Details;

            // list view detail 속성 위한 헤더 추가
            listView1.Columns.Add("이름", listView1.Width / 4, HorizontalAlignment.Left);
            listView1.Columns.Add("수정한 날짜", listView1.Width / 4, HorizontalAlignment.Left);
            listView1.Columns.Add("유형", listView1.Width / 4, HorizontalAlignment.Left);
            listView1.Columns.Add("크기", listView1.Width / 4, HorizontalAlignment.Left);

            // 행 단위 선택 가능
            listView1.FullRowSelect = true;
        }

        private void Fill(TreeNode dirNode)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(dirNode.FullPath);
                // 드라이브의 하위 폴더 추가
                foreach( DirectoryInfo dirItem in dir.GetDirectories() )
                {
                    TreeNode newNode = new TreeNode(dirItem.Name);
                    newNode.ImageIndex = 2;
                    newNode.SelectedImageIndex = 2;
                    dirNode.Nodes.Add(newNode);
                    newNode.Nodes.Add("*");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("catch error: " + ex.Message);
            }
        }

        // 트리 확장되기 전 발생하는 이벤트
        private void treeView1_beforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            //if( e.Node.Nodes[0].Text == "*" || )
            if ( e.Node.ImageIndex == 2  )  // closed icon이면, opened icon으로 변경
            {
                e.Node.Nodes.Clear();
                e.Node.ImageIndex = 3;
                e.Node.SelectedImageIndex = 3;
                Fill(e.Node);
            }
        }

        // 트리 닫히기 전 발생 이벤트
        private void treeView1_beforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            //if( e.Node.Nodes[0].Text == "*" )
            if ( e.Node.ImageIndex == 3  )  // opened icon이면, closed icon 으로 변경
            {
                e.Node.ImageIndex = 2;
                e.Node.SelectedImageIndex = 2;
            }
        }

        // 트리 클릭시 발생 이벤트
        private void treeView1_nodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            settingListView(e.Node.FullPath);
        }

        // 우측 list view 그리기
        private void settingListView(string fullPath)
        {
            try
            {
                // 기존 파일 목록 제거
                listView1.Items.Clear();
                // 현재 경로 표시
                txtCurPath.Text = fullPath;
                DirectoryInfo dir = new DirectoryInfo(fullPath);

                int directCount = 0;
                // 하위 디렉토리 보여주기
                foreach( DirectoryInfo dirItem in dir.GetDirectories() )
                {
                    // 하위 디렉토리 존재하면, list view에 추가
                    // list view item 객체 생성
                    ListViewItem lvItem = new ListViewItem();

                    // 생성된 list view item 객체에 똑같은 이미지 할당
                    //dirItem.Attributes.GetType()
                    // todo icon 이미지 동적 할당
                    lvItem.ImageIndex = 2;
                    lvItem.Text = dirItem.Name;

                    // 아이템을 list view에 추가
                    listView1.Items.Add(lvItem);

                    listView1.Items[directCount].SubItems.Add(dirItem.CreationTime.ToString());
                    listView1.Items[directCount].SubItems.Add("폴더");
                    listView1.Items[directCount].SubItems.Add(dirItem.GetFiles().Length.ToString() + " files");
                    directCount++;
                }

                // 디렉토리에 존재하는 파일목록 보여주기
                FileInfo[] files = dir.GetFiles();
                int count = 0;
                foreach( FileInfo fileinfo in files )
                {
                    ListViewItem lvItem = new ListViewItem();
                    lvItem.ImageIndex = 4;
                    lvItem.Text = fileinfo.Name;
                    listView1.Items.Add(lvItem);
                    //listView1.Items.Add(fileinfo.Name);

                    if (fileinfo.LastWriteTime != null)
                    {
                        listView1.Items[count].SubItems.Add(fileinfo.LastWriteTime.ToString());
                    }
                    else
                    {
                        listView1.Items[count].SubItems.Add(fileinfo.CreationTime.ToString());
                    }

                    listView1.Items[count].SubItems.Add(fileinfo.Attributes.ToString());
                    listView1.Items[count].SubItems.Add(fileinfo.Length.ToString());
                    count++;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("catch error: " + ex.Message);
            }
        }

        // list view mode 변경 처리
        private void cboListViewMode_selectedIndexChanged(object sender, EventArgs e)
        {
            switch( cboListViewMode.Text)
            {
                case "Details":
                    listView1.View = View.Details;
                    break;
                case "SmallIcon":
                    listView1.View = View.SmallIcon;
                    break;
                case "LargeIcon":
                    listView1.View = View.LargeIcon;
                    break;
                case "List":
                    listView1.View = View.List;
                    break;
                case "Tile":
                    listView1.View = View.Tile;
                    break;
            }
        }



    }
}
