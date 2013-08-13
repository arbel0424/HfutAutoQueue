// ==UserScript==
// @name        hfutAotuPost
// @namespace   cn.edu.hfut.auto
// @description 自动向财务处提交
// @include     http://cwcx.hfut.edu.cn/baobiao/Queue/QueueSystem.aspx?deptID=1&dateType=Today&timeType=AM
// @require     https://raw.github.com/miniflycn/JsCV/master/build/core.js
// @version     1
// ==/UserScript==

        var img = document.getElementById("yzm")
        img.onload= function(){

            var model = new Array;
            model[0] = [7, [0, 4080, 1020, 1020, 1020, 1020, 1020, 4080]];
            model[1] = [4, [0, 510, 510, 510, 6120]];
            model[2] = [7, [0, 1020, 1530, 1530, 1530, 1530, 2550, 2040]];
            model[3] = [7, [0, 1530, 1020, 1530, 1530, 2040, 3060, 1530]];
            model[4] = [8, [0, 1020, 1020, 1020, 1530, 1020, 1020, 6120, 510]];
            model[5] = [7, [0, 2040, 2550, 1530, 1530, 1530, 1530, 2550]];
            model[6] = [7, [0, 4080, 1530, 1530, 1530, 1530, 1530, 2550]];
            model[7] = [7, [0, 510, 510, 2040, 2550, 1530, 1530, 510]];
            model[8] = [7, [0, 2550, 2040, 1530, 1530, 1530, 2040, 2550]];
            model[9] = [7, [0, 2550, 1530, 1530, 1530, 1530, 1530, 4080]];


            var mat = cv.imread(img);
            if (mat)
            {
              GM_log('success to get the image');
            }
            var pic_data = mat.data;
            var size_x = mat.col;
            var size_y = mat.row;
            var pix_sum = new Array;
            for (var i = 0; i < size_x; i++)
            {
                pix_sum[i] = 0;
                for (var j = 0; j < size_y; j++)
                {
                    pix_sum[i] += pic_data[(j*size_x+i)*4]
                                + pic_data[(j*size_x+i)*4 + 1]
                                + pic_data[(j*size_x+i)*4 + 2]
                                + pic_data[(j*size_x+i)*4 + 3];
                }
            }
            var x_left = -1;
            var x_right = 0;
            var result = ""
            for (var k = 0; k < 4; k++)
            {
                for (var i = x_left+1; i < size_x; i++)
                {
                    if (pix_sum[i] == 0)
                        x_left = i;
                    else
                        break;
                }
                x_right = x_left;
                for (var i = x_left + 1; i < size_x; i++)
                {
                    if (pix_sum[i] != 0)
                        x_right = i;
                    else
                        break;
                }
                // find the number
                for (var i = 0; i < 10; i++)
                {
                    if (x_right - x_left == model[i][0])
                    {
                        var m = 1
                        for (var j = 0; j < model[i][0]; j++)
                        {
                            if (model[i][1][j] != pix_sum[x_left+j])
                            {
                                m = 0;
                                break;
                            }
                        }
                        if (m == 1)
                        {
                            result += i;
                        }
                    }
                }
                x_left = x_right;
            }
            document.getElementById("Txt_Yzm").value = result;
        };
img.src = img.src+'?';
//form = document.getElementById("form1");
//form.submit();
