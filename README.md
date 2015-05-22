# Douban
一个展示豆瓣Top250电影详细信息以及最新影评的Win8.1Metro小应用，使用sqlite数据库，实现了“搜索，动画，共享，网络，数据存取，磁贴，多线程”
## 使用须知：
1. 本项目使用了sqlite数据库，所以你可能需要先了解一下win8项目使用sqlite数据库的相关配置。<br/>
参考：http://www.devdiv.com/metro%E4%B8%ADsqlite%E5%AE%89%E8%A3%85%E5%BF%83%E5%BE%97-weblog-252306-13068.html<br/>
2. 如果更换数据库，将创建好的数据库导入项目中后，需设置其“生成操作”属性设置为“内容”，“复制到输出目录”选择“始终复制”，否则无法读取项目目录<br/>
可以参考：http://wp.qmatteoq.com/import-an-already-existing-sqlite-database-in-a-windows-8-application/<br/>
3. 代码中有其他相关的简要注释与说明，在此不多说了。

## 安装应用：
本项目已经有创建好的应用程序包，如想安装，请进入\Douban\Douban.Windows\AppPackages\Douban.Windows_1.0.0.1_Debug_Test，在Add-AppDevPackage.ps1右键选择
使用PowerShell运行，按要求Yes或者Enter，便可最终安装成功。<br/>
关于win8应用打包，可参考：http://blog.csdn.net/bluecloudmatrix/article/details/34853699

## 应用效果图：
1. 初始页面，左边为豆瓣电影Top250，右边为基本介绍
![](/Picts/1.jpg)
2. 选中电影，右边显示电影详细信息
![](/Picts/2.jpg)
3. 搜索电影（带搜索提示），确认后选中搜索的电影并显示器详细信息
![](/Picts/3.jpg)
4. 下边栏中可对电影进行排序
![](/Picts/4.jpg)
5. 点击图4中上边栏的按钮可跳转到最新热门影评展示界面
![](/Picts/5.jpg)
6. 选中某一影评可对其进行分享
![](/Picts/6.jpg)
7. 当屏幕被放到一边时，页面显示方式变化
![](/Picts/7.jpg)
8. 当由于网络原因获取不到最新影评时，可点击刷新按钮来进行刷新
![](/Picts/8.jpg)
