﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Soomes;
using AliHelper.Bussness;
using System.Collections;
using AliHelper.Http;

namespace AliHelper
{
    public partial class MainForm : Form
    {
        //alibaba vip manage url
        public static string url = "http://hz.productposting.alibaba.com/product/manage_products.htm#tab=approved";


        public static string CsrfToken = string.Empty;

        public ProductsManager productsManager;
        
        public MainForm()
        {
            InitializeComponent();
            productsManager = new ProductsManager();
            string html = IEHandleUtils.WebRequestGetUrlHtml(url);
            CsrfToken = GetCsrfToken(html);

            //  IEHandleUtils.WebBrowerSetCookies_NavigateToUrl(this.webBrowser1, url);
            //  this.webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            List<AliGroup> groups = productsManager.GetGroupList();
            UpdateGroupUI(groups);
            
        }

        void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser browser = (WebBrowser)sender;
            if (browser.ReadyState != System.Windows.Forms.WebBrowserReadyState.Complete)
                return;
            if (e.Url.ToString() != browser.Url.ToString())
                return;
            if (browser.Url.ToString() == url)
            {

            }
            System.Diagnostics.Trace.WriteLine("========================" + this.webBrowser1.Url.ToString());
        }


        public string GetCsrfToken(string html)
        {
            Regex r = new Regex("var _csrf_ = {'_csrf_token_':'(.*?)'};");
            GroupCollection gc = r.Match(html).Groups;
            if (gc != null && gc.Count > 1)
            {
                return gc[1].Value.Trim();
            }
            return "";
        }

        public void UpdateGroupUI(List<AliGroup> groups)
        {
            treeView1.Nodes.Clear();
            TreeNode t = new TreeNode("产品分组");//作为根节点
            treeView1.Nodes.Add(t);
            foreach (AliGroup p in groups)
            {
                if (p.Level == 1)
                {
                    TreeNode t1 = new TreeNode(p.Name);
                    t1.Tag = p.Id;
                    t.Nodes.Add(t1);
                    if (p.HasChildren)
                    {
                        foreach (AliGroup c in groups)
                        {
                            if (c.ParentId == p.Id && c.Level == p.Level + 1)
                            {
                                TreeNode t2 = new TreeNode(c.Name);
                                t2.Tag = c.Id;
                                t1.Nodes.Add(t2);
                            }
                        }
                    }
                }
                
            }
            treeView1.ExpandAll();
        }


        private void updateGroup_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CsrfToken))
            {
                return;
            }
            List<AliGroup> groups = HttpClient.GetGroups(-1, 0, CsrfToken);
            productsManager.UpdateGroups(groups);
            UpdateGroupUI(groups);
        }

        private void updateAllProduct_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CsrfToken))
            {
                return;
            }
            List<AliGroup> groups = HttpClient.GetGroups(-1, 0, CsrfToken);
            productsManager.UpdateGroups(groups);
            UpdateGroupUI(groups);
            GetGroupProduct(groups, CsrfToken);
        }

        public List<AliProduct> GetGroupProduct(List<AliGroup> groups, string csrfToken)
        {
            List<AliProduct> produtList = new List<AliProduct>();
            Hashtable groupDic = new Hashtable();
            foreach (AliGroup group in groups)
            {
                groupDic.Add(group.Name, group.Id);
            }
            foreach (AliGroup group in groups)
            {
                if (group.Level == 1)
                {
                    List<AliProduct> products = HttpClient.GetProducts(groupDic, group.Id, group.Level, csrfToken);
                    productsManager.UpdateGroupProdcuts(group.Id, products);
                    produtList.AddRange(products);
                }
            }
            return produtList;
        }

        public List<AliProduct> GetGroupProduct(List<AliGroup> groups, AliGroup currGroup, string csrfToken)
        {
            List<AliProduct> produtList = new List<AliProduct>();
            Hashtable groupDic = new Hashtable();
            foreach (AliGroup group in groups)
            {
                groupDic.Add(group.Name, group.Id);
            }
            List<AliProduct> products = HttpClient.GetProducts(groupDic, currGroup.Id, currGroup.Level, csrfToken);
            productsManager.UpdateGroupProdcuts(currGroup.Id, products);
            produtList.AddRange(products);

            return produtList;
        }
        

        

        

    }
}
