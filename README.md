# VSIXChineseCompletion
Visual Studio (CSharp) 中文代码补全 (使用拼音补全中文)

# 背景
--> 许多业务逻辑中包含复杂的词汇,对此做英文变量命名非常耗费脑力,并且在日后维护中容易忘记,注释也只能提醒变量当前文件,对整个工程不友好  
--> 在代码中使用中文需要切换输入法,频繁切换输入法比较费劲,为此需要在第一次为变量命名输完中文后,后续使用拼音作为代码提示的关键词,以大幅节省输入法切换次数  

# 基础使用
[下载查看Release](https://github.com/sharpoverflow/VSIXChineseCompletion/releases)  

![image](https://github.com/sharpoverflow/VSIXChineseCompletion/blob/main/GitImage/%E8%BE%93%E5%85%A51.jpg)
![image](https://github.com/sharpoverflow/VSIXChineseCompletion/blob/main/GitImage/%E8%BE%93%E5%85%A52.jpg)

--> 多音字问题不可避免,但是不影响使用  

![image](https://github.com/sharpoverflow/VSIXChineseCompletion/blob/main/GitImage/%E5%A4%9A%E9%9F%B3%E5%AD%97.jpg)

# 备注
因为VS没有将原始CSharp代码提示的获取接口开放,这里使用反射获取,并且由于相关资料甚少,很难进一步推断优化方法  
工具在VS2019下测试通过,其他版本情况未知
