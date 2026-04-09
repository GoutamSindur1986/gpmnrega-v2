function generategeotag(resp) {
    try {
        var blnakImage = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAcIAAAFjCAYAAACuWAsOAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAFiUAABYlAUlSJPAAAA2ZSURBVHhe7dy/jxtlAsfhd3cTAl1uIaKhAN2FFFRRhJRQXINQJJSGgiYt3Un8Dav7G5DoaLehuAYhRYiG4kA6RakocgGRIh2QS0eSTXZvxp6xvbbHa3ucu8D3eaQV3o09tsfvzGd+ma29vb2jAgChtpv/AkAkIQQgmhACEE0IAYgmhABEE0IAogkhANGEEIBoQghANCEEIJoQAhBNCAGIJoQARBNCAKIJIQDRhBCAaEIIQDQhBCCaEAIQTQgBiCaEAEQTQgCiCSEA0YQQgGhCCEA0IQQgmhACEE0IAYgmhABEE0IAogkhANGEEIBoQghANCEEIJoQAhBNCAGIJoQARBNCAKIJIQDRhBCAaEIIQDQhBCCaEAIQTQgBiCaEAEQTQgCiCSEA0YQQgGhCCEA0IQQgmhACEE0IAYgmhABEE0IAogkhANGEEIBoQghANCEEIJoQAhBNCAGIJoQARBNCAKIJIQDRhBCAaEIIQDQhBCCaEAIQTQgBiCaEAEQTQgCiCSEA0YQQgGhCCEA0IQQgmhACEE0IAYgmhABEE0IAogkhANGEEIBoQghANCEEIJoQAhBNCAGIJoQARBNCAKIJIQDRhBCAaEIIQDQhBCCaEAIQTQgBiCaEAEQTQgCiCSEA0YQQgGhCCEA0IQQgmhACEE0IAYgmhABEE0IAogkhANGEEIBoQghANCEEIJoQAhBNCAGIJoQARBNCAKIJIQDRhBCAaEIIQDQhBCCaEAIQTQgBiCaEAEQTQgCiCSEA0YQQgGhCCEA0IQQgmhACEE0IAYgmhABEE0IAogkhANGEEIBoQghANCEEIJoQAhBNCAGIJoQARBNCAKIJIQDRhBCAaEIIQDQhBCCaEAIQTQgBiCaEAETb2tvbO2pus7S75YW/f1l26puX3i+/XXt98NfW9hefljM3qxsz//agbH93s5z+/nbZvtf86bVz5elf3ysH58+WZT6IrTtflxe+mXh8OVcOL/2lHLxzsRzuNn+afH0z6vu/XR5Xr2v6+QbT3q+m3fx++NqVcvDB5HSXt3XnVjn9zQ9l597PzV/mvc7WsvPl+Ps6vHq9PLp8tvmtes7vPi8v3hg/38HHH5Ynu6vMi/F9n17/W3l8fvDHORZNs3WhPNp7txxWt0bjYYHDq1fK0Y1vl5zmguev59tbb5eDy7Of79hJr7+dd82va34+M5rHPK4e02XVeTX3c6rG8UvVOB7Nr/u3yplPvq3G9fT76jfml3nscu9nYhyPXmttPIbm6b8uaC1+ngT2CPu6+WV54U5ze6EH5dRn++XMjcmBW6lCsbO/X1787FbZav40X/34T8uL9YI3+fjyc9m++W218HxeTt1v/rRQff8vy4tf3G1+HxpEZGKhrm3fq6f79cqDZPuLelrVimoUwVrX61x/vmx//9Oxf9v6ZfL5ljF/Xvyu1fPtRvWeThxPy+o7bic0j1lueXn2+oz5TS4vk7b+/cPE42+XU3Pn1abWBbTsEa5leivr+BbVvD3C8d5KtVV8fbxVPLlVOb2HM2mZx4+fb/z6jm8x11v2X1UrteF0xlvHw5Xd6WqhOqymMdhDul9N45PhNBa9rhnV6xlujU+/zlvV62y2dKst54cfXRzsSaw2XxbN9+l/m90jPHledN132rL3m6Nz72SVaXbfd7nxtPxzrfv5zE53PMYmP/+F1p1XozG4aI+wz5hf87EL9kyH2ulW8/pSKTs3q/k+sQ5pbWZdwKTBPKOv2+X0dw+a2/PcLacHA7cejB+OBm7t6Py75dH1C4Pb2zdudnwgix//+Gp9SORKefTO8QVm1tlyePnt8nRwu9p6/HVw47hXmkNdu6+XJ9XCWNv+ZdF7m1QtyN/UK6B6ZTBeQGtH5y+OX+cH7Upw3flSryjOVf+d2GK+8+NgYS+XLjTv7yRLzIvfoXY81LrH07L6jttJZ8uTvw7v/9zpM+Z7LS9T7v9UdgYbCvXhzb8MN/Bu/jg1bze1LmBSv+WEaoXcrgy+6j4c0a6kqy3UJ/O2yM7/uVkh3y/b86Zx/z/N4af5jz+6/GF5dG2Z8xr1XtC/mtdSLTAvD25Uqii8Ory1fWO/nPns6+p1PCiH1/5Wfturfqa2SDu1C3IdqjfHC2hr5nX2mC+HrwwnsvXrcKWz9evwDu3fT9Y1L37/jt5sVqJd42lZfcftMeONpPLqn07eG3zm+oz5DS0vU0aHRev5s/tGefpa/cvU4dGNrQuYJIR9XbhUDgYD9udy+h+3Fp8veW23YwVQLVjNNObumVQr+cEH1fn4bjv7n5aX/t7+1Od6hluT5dLbxw7NHF57f7QntX3vdjnzyX556bPPywt31tm63V1tQVxnvrw5XAkPzxM+KDvf1++rDvCfBv88z7LzYhXHpzn+ObPwCMFivae5u3xo5j3XzPOs8fnMTrc5LFp9Rgcb3FuZO68Gh0VP1mfMb3Z5qbVjuNq4vlDPn7Pl6VvDPfud2xPnsDe2Lhj/9BmrfxRC2NvZ8uSDK8Mt8HvfltP/swsB6vMJ04N6iRP19ZV7V98vD2e2Wl8vj/eul0dXL4zOdbYXNzyfC0qzEr73Q9mptsaHFw2sGuCuecHmNfN6b965sf+XPmN+w8vL6GjKeE9vtGc/c3h02prrAkbMq03YvVgOmvMDO/tfl1PDm7Pu3e/YY2xX5B2H6F6uVvD1fzsf360+QT44XNP+fPRhedx5aX0Vl8vvlkf1/T6utngHW/vVIFn5XNOKh+TWmi/tFnO1N/LP9vzgn8crpTlWmxfLmZlm89N9ocXJek9zdPjsZPOea+Z51vh82uk+bM4j1p/T1i/NzQ2aO69Gz7mMPmN+U8vLxGHRUu1dtjEbfY1i4vDoJtcFzU+fsfpHsernRYfxoZLbZWf6e0OjcynzL6rZGp2r6tijGR3qmnx8tSf6UTOYV1rw56ivspveityttnjbPd1lwzY6r1FtGf97zlZx9Tzt+ZSBnvPlqFop1HZuNhfovGKBHmgPn626hzyt77it1BdwPGwv3qm/arTO3tKz0GfMb2p5GRkfFu0yOjz6rNcFoYRwY6oFoXMQTlxRduOrY+cRBpc8N+eqDq9e6tijeb0cjK4EPP74cv9uOXV7paVu1rEV3t1mS7Pa2h9tpS67Qh2f15h9n7fKmfrS7vp8yj/a7//1nC+j112bf4FOmnY+17rH07L6jtuho8vvNefRh9N5Lr7j1mfMb2x5aUxcZHbwcRO09qddp4wOjz7jdUEo3yNcS9d3c9rvATW/HvsO0NS/TTvxu1XVgvbF/sL/S0X9naZHg+db/btD4+8mzVr8napZ9Rfqz9TfgZpr+jtUq8yX9n2105h8bPudwun7rDIvxvftMpwXD0683+z7bCzx3bhus++p08LxtMo8WefzmTPd0fuuPBffI+w35td67EmvY+58mfde11sXdJuev3mGGxlsyMSFMzPqwxfNyfVm63igvWDjxBXD2cHl2Q+vXxmdixg6N/zeULUlORz466kvu67P58y+ttUiWDu8Vk+r63VOL3B95st4D/Sk84Mxlh5Py+o7bhu7F0ffO6wvKnseDpH2GfObW17Gh0UP33pjzrwc75WPrx59tuuCRPYIAYhmjxCAaEIIQDQhBCCaEAIQTQgBiCaEAEQTQgCiCSEA0YQQgGhCCEA0IQQgmhACEE0IAYgmhABEE0IAogkhANGEEIBoQghANCEEIJoQAhBNCAGIJoQARBNCAKIJIQDRhBCAaEIIQDQhBCCaEAIQTQgBiCaEAEQTQgCiCSEA0YQQgGhCCEA0IQQgmhACEE0IAYgmhABEE0IAogkhANGEEIBoQghANCEEIJoQAhBNCAGIJoQARBNCAKIJIQDRhBCAaEIIQDQhBCCaEAIQTQgBiCaEAEQTQgCiCSEA0YQQgGhCCEA0IQQgmhACEE0IAYgmhABEE0IAogkhANGEEIBoQghANCEEIJoQAhBNCAGIJoQARBNCAKIJIQDRhBCAaEIIQDQhBCCaEAIQTQgBiCaEAEQTQgCiCSEA0YQQgGhCCEA0IQQgmhACEE0IAYgmhABEE0IAogkhANGEEIBoQghANCEEIJoQAhBNCAGIJoQARBNCAKIJIQDRhBCAaEIIQDQhBCCaEAIQTQgBiCaEAEQTQgCiCSEA0YQQgGhCCEA0IQQgmhACEE0IAYgmhABEE0IAogkhANGEEIBoQghANCEEIJoQAhBNCAGIJoQARBNCAKIJIQDRhBCAaEIIQDQhBCCaEAIQTQgBiCaEAEQTQgCiCSEA0YQQgGhCCEA0IQQgmhACEE0IAYgmhABEE0IAogkhANGEEIBoQghANCEEIJoQAhBNCAGIJoQARBNCAKIJIQDRhBCAaEIIQDQhBCCaEAIQTQgBiCaEAEQTQgCiCSEA0YQQgGhCCEA0IQQgmhACEE0IAYgmhABEE0IAogkhANGEEIBoQghANCEEIJoQAhBNCAGIJoQARBNCAKIJIQDRhBCAaEIIQDQhBCCaEAIQTQgBiCaEAEQTQgCiCSEA0YQQgGhCCEA0IQQgmhACEE0IAYgmhABEE0IAogkhANGEEIBoQghANCEEIJoQAhBNCAGIJoQARBNCAKIJIQDRhBCAaEIIQDQhBCCaEAIQTQgBiCaEAEQTQgCiCSEAwUr5L6RfJV29I4IsAAAAAElFTkSuQmCC';
        var docDefinition = {
            defaultStyle: {
                font: 'tunga',
            },
            background: [

            ],
            content: [

                {
                    canvas: [
                        {
                            type: 'line',
                            x1: -10, y1: 0,
                            x2: -10, y2: 760,
                            lineWidth: 1,
                            color: '#b8860b'
                        },
                        {
                            type: 'line',
                            x1: -10, y1: 0,
                            x2: 530, y2: 0,
                            lineWidth: 5,
                            color: '#b8860b'
                        },
                        {
                            type: 'line',
                            x1: 530, y1: 0,
                            x2: 530, y2: 760,
                            lineWidth: 5,
                            color: '#b8860b'
                        },
                        {
                            type: 'line',
                            x1: -10, y1: 760,
                            x2: 530, y2: 760,
                            lineWidth: 5,
                            color: '#b8860b'
                        }

                    ]
                }
                ,
                {
                    text: 'GEOTAG REPORT',
                    fontSize: 15,
                    color: '#b8860b',
                    bold: true,
                    absolutePosition: { x: 220, y: 60 }
                },
                {
                    image: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAASoAAACpCAMAAACrt4DfAAABuVBMVEX///9/yUxMoMnJoUxsq0HU0nz595KriUFBiKugAADcAADpAADZAADmAADqAADVAADQAADgAADLAADZ1Xl7x0XLo01FnceaAACXAAD/+5iiAABIl777//nxAAC9AACQ1GJ5wEizAAC9xYu+mEj4/vRntUUyg6/EAAC0AACH0FTs6op0t0bTrFv//I5nqDnCamp81FDX64RlsNWsMzPkvLz88/Py3d29Xl7NhobT8cFkqTGSTB3E43mh1WTv9JB9sU05mc39/bnHnD3u9vqngzXoy8vZo6OhFxfFdXW0S0vv1NSxPT2sLCzSlJTcqqqoIiLw+uma2HG956OoWDq07p+DtUPp8+OWyHapz5K926zP58Dg89Ss3ox2yDmIv2WcxoOXWCHC56yo34WUZyeRfC+006PHvZ+NkDebIw1ynz2Joz2aQBmRUR+IbSl/hTODey95kDbC2Xanv2bCynSuw2rfyHF9nkW1d0aqSSvk7p+puY7MzoPFpmKOvr1dlKd0n6KMqp2ky7XEyoaWr5ivz7N1ssKRwLzFuGvr1KfauHXx48jjyZeLw+C02e2pxtdtob3t7MG7oGnl2shJXpOVAAAKlElEQVR4nO3di1/UVhYHcFCceuedcSQhxdAoQwZQaWEGKEWkvBTROrorPig+6ta2S0sfW7f10e5uu62u1Yr+xXtvknlkcm9yk8ljzM3PT/lQOtWb7+eckzPD8LGnp4tTXV2bnZk6I/DCmfMzsxdWp8M+UJdmdfa8AGQln+YymQyXziuyKJw5W6mGfa5uy/Qsz5dz2SPGZNMS4CdWwz5cN2V1XSgnj+DDScJUJewDdkvObQgKrKcsPlCrwF+MsWCqE4JEdKprFYT1eMRXgJyyhlKxspKwFvZRQ85ZIW8PpWIlhXWWb4bVKZGipOpdKPPsNuG0KNNCqViSwOreMA0kJ1LQqsColWMpaJVn0qrqXEqtKwbn1XlHc6phpQDm7oMT4pGUmxyRN8I+esCpCJmsK6pUFsyGffhAU+XzLqVSWU44F/bxg8yM7K79VCvlfNjHDzCrQsa1FGpBhp4OTinZDEwqlUG/tA+NT1KpjP5vqYzhQ+OTHDt3wQqvarhOVrwQ9iUEFb2oXAeWVdiXEFDOCTYSKNZlBT4K+yKCyUTZoqjgMpBXJKWQtNJKMXITrPJJi4JSRJ4HAPC8KGUssNh4KljhiUWVzQPkpIcnj7SszMRgn5CIAFILFLKSiXVVmAr7MoLImRzh8lNtUjBEK05gYLWaJt7/CiYpwEsEqyxg4DuDFZFUKWYpaJUmPJiFYTUrEy6+jJECgASrzIR9If5nRnFQVAAIhM0iz8Bc3yhkOJiM/o/+AV47ngpImZYHNj9Ji9Gf6/AGyGGSkfBSQMY+nGPhFiim8VQygQqksFIM7OvwaQ3+0kUSFb6qMtF/2dgzKj7yVD2AQEVqQJFUVZFvwB4xh710TonHens2CvhLzxGWBcLDWVgWJiT8tROGFY9/NFdg4JvMa4Thw2GeLcMQYDnpbNgX4n/OCYSLx5cV8cEMvLpeBYS5ziUx7Ud6LMfADRA+Xyb1FJczSRFmOuzWi2FfRhD5CGSSySSn/1P/oP5Ki8Z5lefaHlj/hJOZeDtMVUirV4wJnNZNK76cJD0uycq7YdYlIgHEUmTAw4gKGSrJFcSwLyKYVAAZAWFxybT60eIxIhP9B8PnLRhokmbi/odyVrYqGftw0nrYlxBUpoV0Z1Q8A9/Z0mM12ClS4MO+gOBS4ZNp9+HYeScazEXFvVUyz0f/9Zdm1gDnvqhk9UWF2p1Lo8NhX0cQAQXXVDm4KWxdHtouFotMWF0Q3ZZVsvyXv45uF3tRmLCq8nm3RQWuaE7MWM2K7gZ7srzzbm8vU1ZV3uW0Ald6exmzcllW5Q/e7X0zrGqbXh3MXVnl2oqqK60GN9+/fAneeub6bl69dv3GVq3TA7oqK1NRdZnV8Nadj4eKaJGBB3un7yTM3NzcSc1ss+byd0VllXMac1F1jVXtxuVPitvbxeb9GVo1opnNjd+8de36luPWvACSDqHS8m1zUYVvNbz1/seX6qVkSItVm9npq7c+vbG1OThI90eIisOyyoN3cFIhWtW27nwyhFUiWrWX2dVryMzm/Gu8M6q0fBdbVOFY1bTZTUKytWozq48zQpnxkiOrAiAfKUgrdXZv2yrRWlGNswrvqKjEv5GKKjArNLtHjbPbO6t2s6u3rqHW1Opso+ygrBTe8kg+W6mz26GSW6smGTIbV28B9/i8g6L6zPqUvlnZzW7frFrLrLQjU5eV9LlF+/llhWZ3B0heWaF8AQq0VODvtkfy2Ooy9ewOwqp0W8zlaZKTd+yKynOrwSEvmDyz6gMKjRXcPnFPaZiyKn1FVVY58vbJkBUvUVgpFtunn1Y9XWW1K1IUleX26adVV9VVaadsW1aSQC0VaSu4MNhRgc+cHCm6VqUvZeuyypVpFgUmrODCYElVoFsUWLCyWRhyhNc+2bQCkgWVQnrtk0kry4XBwaLAgFXp8zJRSqLePtmw+idcGArY5EVHi0L0rUofyHiqfNn2ZSrWrL4ACr6qKF6mYsyqdFfElVVexnznnXkrIGGoXC0KLYmm1deYssqLlC9TERNJK7gwYIqq4yNF0uob0VxUrrZPY6JoVdqR26gkvnOpaFrBPdQY19unMRG0gnuoQaqD7dOQYvEU5duY3hyrtj1U3B3v/DTF3lPvJQYGomZV+lI2FFWpr0Or4uj8ewMDCZjIWfWJLWUlfgO/4N6qWByaX1CZElG0Kt1ulpUMi8qtVRG1XYtTFK1aykotKldWxdFTettF2apZVmWtqJxaaW1ndupqq5OurBo3wXpRObAqwq1gPoFn6lYrOCqG5r/97rvTp7V36TkpK323ahYVrZW2FZCdus0K/ejnqXk4UWHGDqj5x/eqGa2YvrKLu61ftLOyarsutIJKQ1AJrnv1I+tW9ahmfXZmpR30AoMESoavWlmhtlugYUocur//eLhW6E2AeikZD9dmVSeDZlqZYc120QsM4ldGKqIVTdupTmP3j+9HCcsKIekNhz0g3qpZZt9jxhl6OVQRTYIYq2JzGbd3OrZfT/BWSAk23ELCVEv0VobWrJuV7sqF8k7Jzop2PKltZ0igVvrsTlCclM7KYAYHu7iL6c3xlj+/aFrGCU5624Vg1Zzd9ud0Y4UyJpR5aHbT1Jq6FRpPtG1ndgrAiji77a0OObT6AfxY/1/0W4B+2xx3sBWM3ccz+WrVMrsdKiVOnDix8ODTh0cfHTiEQkn1mP/B9DXtFnCFdisglJOPVmopabPbIZKm9N+fJvsXFxdHRg4e/Plf//714S+PaMieCI9J/2nM3onYdr5auSslpPTg+n9+27e42N+/D+Xtg2pGUKDZrw+P/mJZZsITIqO1lXXb+WfV8+EJF0iJB9d//22xobSv1aoRzQyWGWpNs8ePFiVHtDJvBYFaOVJKLNyBDYeQWpXwVi1kR81Wlh2KtaJsu9Ct1FJqbThMCFaIC2NlGZOVG6fArbTZ/ftke8MFZzUw4Kzt/LQizSvT7LaLY6u3YB4/eYIZ8XUriq0gXCvS7PbFCmYFkRnNxhJtz4FdW3lJZehB1HDa7Ham1JlVa3SysbH7hztl0qx8mFeOG84nKz3HPIHy3go2nDq7cWtAaFbeVJXXVpOdlBJjdTXpDZTHVl1aV9G28pIqtoqtYivnVvG8orfykirqVnFd0Vt5SRVbxVZ1q7gHY6vYKraKrWKr2Cq2iq1iq9gqtoqtYqvYipylpcNPvaSKqtXS0vGnm55CRdNqael/zzztvWhawbb745kfTBGzQm3nn1N0rGDbPfX/r+F48618G0/+WfWHYAXbzsfx5INVP3rn1uTzFy9e/Pn8JQQbCcYKtZ3nW4F/VnWl5Zbfb3n5xZ+q2YiPVsG1XedWCKkfldIy8fdVzV6+fBv9kI6XVv5uBdZxaFVXWiYrGYPMnh89cADzgzmOrfzfCqxDbaU1nFUpWWVweHNvb+/Bo1YzR1ao7cL+ixftrRqz252SMYPDtb29Dx+pPzS3srJCZ4XaLoTxZIqVFW52e5TBWm3v3h+vjq1oIVoFvBVYB2ulTqV9HpWSZYZfv35279Xjt4xkmlUwy7iDtFk1Ssl3JUMGh18/e9o0O3Z46XAoW4F1GlY+Nhx9kBlszVdd03aGTPZTLEtx1Ey6XgO6L/8HwQbTmEsFnMAAAAAASUVORK5CYII=',
                    width: 50,

                    absolutePosition: { x: 340, y: 50 }
                },
                {
                    absolutePosition: { x: 60, y: 100 },
                    columns: [
                        {
                            // auto-sized columns have their widths based on their content
                            width: '25%',
                            text: 'ಪಂಚಾಯತಿ: ' + $(resp).find('span#txtPanch').text(),
                            fontSize: 10
                        },
                        {
                            // star-sized columns fill the remaining space
                            // if there's more than one star-column, available width is divided equally
                            width: '25%',
                            text: 'ತಾಲ್ಲೂಕು: ' + $(resp).find('span#txtblock').text(), fontSize: 10
                        },
                        {
                            // fixed width
                            width: '25%',
                            text: 'ಜಿಲ್ಲೆ: ' + $(resp).find('span#txtdist').text(), fontSize: 10
                        },
                        {
                            // % width
                            width: '25%',
                            text: 'ರಾಜ್ಯ: KARNATAKA', fontSize: 10
                        }
                    ]
                },
                {
                    absolutePosition: { x: 38, y: 30 },
                    canvas: [
                        {
                            type: 'line',
                            x1: 0, y1: 100,
                            x2: 528, y2: 100,
                            lineWidth: 5,
                            color: '#b8860b'
                        }
                    ]
                },
                {
                    absolutePosition: { x: 60, y: 150 },
                    columns: [
                        {
                            // auto-sized columns have their widths based on their content
                            width: 'auto',
                            text: 'Work Name: ' + $(resp).find('span#txtWn').text(),
                            fontSize: 10,
                            bold: true
                        }
                    ]
                },
                {
                    absolutePosition: { x: 60, y: 170 },
                    columns: [
                        {
                            // auto-sized columns have their widths based on their content
                            width: '60%',
                            text: 'Work Code: ' + $(resp).find('span#txtCode').text() ,
                            fontSize: 10,
                            bold: true
                        },
                        {
                            // star-sized columns fill the remaining space
                            // if there's more than one star-column, available width is divided equally
                            width: '40%',
                            text: 'Finanacial Year: ' + $(resp).find('span#txtfin').text(), fontSize: 10,
                            bold: true
                        }
                    ]
                },
                {
                    absolutePosition: { x: 60, y: 190 },
                    columns: [
                        {
                            // auto-sized columns have their widths based on their content
                            width: '60%',
                            text: 'LAT: ' + $(resp).find('span#txtlat').text() + ' LON: ' + $(resp).find('span#txtlon').text(),
                            fontSize: 10,
                            bold: true
                        },
                        {
                            // star-sized columns fill the remaining space
                            // if there's more than one star-column, available width is divided equally
                            width: '40%',
                            text: 'Date Creation: ' + $(resp).find('span#txtDate').text(),
                            fontSize: 12,
                            bold: true
                        }
                    ]
                },
                {
                    absolutePosition: { x: 270, y: 200 },
                    text: 'STAGE 1',
                    fontSize: 10,
                    bold: true,
                    decoration: 'underline',
                    color: '#b8860b'
                },
                {
                    absolutePosition: { x: 45, y: 220 },
                    widths: [],
                    columns: [
                        {
                            image: $(resp)[5].querySelectorAll('img#stage11').length > 0 ? $(resp)[5].querySelectorAll('img#stage11')[0].src:blnakImage,
                            width: 250,
                            height: 170
                        }, {

                        },
                        {
                            image: $(resp)[5].querySelectorAll('img#stage12').length > 0 ? $(resp)[5].querySelectorAll('img#stage12')[0].src : blnakImage,
                            width: 250,
                            height: 170
                        }
                    ]
                },
                {
                    absolutePosition: { x: 270, y: 400 },
                    text: 'STAGE 2',
                    fontSize: 10,
                    bold: true,
                    decoration: 'underline',
                    color: '#b8860b'
                },
                {
                    absolutePosition: { x: 45, y: 420 },
                    widths: [],
                    columns: [
                        {

                            image: $(resp)[5].querySelectorAll('img#stage21').length > 0 ? $(resp)[5].querySelectorAll('img#stage21')[0].src : blnakImage,
                            width: 250,
                            height: 170
                        }, {

                        },
                        {

                            image: $(resp)[5].querySelectorAll('img#stage22').length > 0 ? $(resp)[5].querySelectorAll('img#stage22')[0].src: blnakImage,
                            width: 250,
                            height: 170
                        }
                    ]
                },
                {
                    absolutePosition: { x: 270, y: 600 },
                    text: 'STAGE 3',
                    fontSize: 10,
                    bold: true,
                    decoration: 'underline',
                    color: '#b8860b'
                },
                {
                    absolutePosition: { x: 45, y: 620 },
                    widths: [],
                    columns: [
                        {
                            // auto-sized columns have their widths based on their content

                            image: $(resp)[5].querySelectorAll('img#stage31').length > 0 ? $(resp)[5].querySelectorAll('img#stage31')[0].src: blnakImage,
                            width: 250,
                            height: 170
                        }, {

                        },
                        {
                            image: $(resp)[5].querySelectorAll('img#stage32').length > 0 ? $(resp)[5].querySelectorAll('img#stage32')[0].src: blnakImage,
                            width: 250,
                            height: 170
                        }
                    ]
                }

            ]
        }

        let x = atob(getCookie('test').split('|')[1]);
        if (x === "No")
            docDefinition.background.push({
                image: 'data:image/png;base64,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',
                width: 700
            })
        generateNewPdf(docDefinition, reportData.workcode + "_geotag");
       
    } catch (e) {
        console.log(e.message);
    }


}