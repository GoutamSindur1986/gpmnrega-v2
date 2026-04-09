function generatechecklist() {

    try {
        var appArray = [
            [
                {
                    text: "ಕ್ರ.ಸಂ",
                    fontSize: 10,
                    style: {
                        alignment: "center",
                        bold: true
                    }
                },
                {
                    text: "ದಾಖಲೆಗಳ ವಿವರ",
                    fontSize: 10,
                    style: {
                        alignment: "center",
                        bold: true
                    }
                },
                {
                    text: "ಹೌದು/ಇಲ್ಲ",
                    fontSize: 10,
                    style: {
                        alignment: "center",
                        bold: true
                    }
                },
                {
                    text: "ಪುಟ ಸಂಖ್ಯೆ",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "1",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಕ್ರೀಯಾ ಯೋಜನೆ ಪ್ರತಿ",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "2",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಅಂದಾಜು ಪತ್ರಿಕೆ ಪ್ರತಿ",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "3",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ನಮೂನೆ-೬ ಕೆಲಸಕ್ಕಾಗಿ ಅರ್ಜಿ ನಮೂನೆ",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "4",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಕಾಮಗಾರಿಯ ಆಡಳಿತಾತ್ಮಕ ಮಂಜೂರಾತಿ ಪ್ರತಿ",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "5",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಕಾಮಗಾರಿ ಆದೇಶ ಪ್ರತಿ",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "6",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ನಮೂನೆ-೮ ಕೇಲಸಕ್ಕಾಗಿ ಹಾಜರಾಗಲುತಿಳಿಸುವ ನೋಟಿಸ್(ನಕಲು ಪ್ರತಿ)",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "7",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ನಮೂನೆ-೯ ಕೆಲಸಕ್ಕಾಗಿ ಹಾಜರಾಗಲು ಸಾರ್ವಜನಿಕ ನೋಟಿಸ್‌",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "8",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಭರ್ತಿ ಮಾಡಿದ ಇ-ಮಸ್ಟರ್‌ರೋಲ್‌ ಹಾಜರಾತಿ ಪಟ್ಟಿ",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "9",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಎಂಐಎಸ್‌ ಮುದ್ರಿತ ಇ-ಮಸ್ಟರ್‌ರೋಲ್‌ ಪ್ರತಿ",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "10",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಅಳತೆ ಪುಸ್ತಕದ ನಕಲು ಪ್ರತಿ.(ಕೂಲಿ+ಸಾಮಾಗ್ರಿ)",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "11",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಸಾಮಾಗ್ರಿ ದರಸೂಚಿ ಆಹ್ವಾನ",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "12",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಸಾಮಗ್ರಿ ದರಪಟ್ಟಿ (ಕೊಟೇಷನ್‌ ಪ್ರತಿ)ಹಾಗೂ ತುಲನಾತ್ಮಕ ಪಟ್ಟಿ",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "13",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಸಾಮಾಗ್ರಿ ಸರಬರಾಜು ಆದೇಶ ಪ್ರತಿ",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "14",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಸಾಮಾಗ್ರಿ ವೆಚ್ಚದ ವೋಚರ್‌ಗಳು ಹಾಗೂಪಾವತಿ ವಿವರಗಳು",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "15",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಸಾಮಾಗ್ರಿ ದಾಸ್ತಾನಮು ವಿತರಣೆ ವಹಿಯಲ್ಲಿದಾಖಲಿಸಿರವ ಸಾಮಾಗ್ರಿ ದಾಸ್ತಾನು ಮತ್ತು ಬಳಕೆ ಪ್ರತಿ",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "16",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ತೆರಿಗೆಪಾವತಿಸಿರುವ ಸ್ವೀಕೃತಿ ಪ್ರತಿ(ರಾಯಲ್ಟಿ, ಜಿಎಸ್ಟಿ)",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "17",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಪಾವತಿಸಿರುವ ಕೂಲಿ ಮತ್ತು ಸಾಮಾಗ್ರಿಗಳ ಫ್‌ಟಿಓ ಪ್ರತಿಗಳು",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "18",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಕಾಮಗಾರಿಯ ೩ ಹಂತದ ಛಾಯಾ ಚಿತ್ರಗಳು",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "19",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಕಾಮಗಾರಿಯ ಸ್ಥಳಗಳಲ್ಲಿ ಹಾಕಿದ ನಾಮಫಲಕದ ವಿವರ",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "20",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಆಸ್ತಿ ಸೃಜನೆಯ ಜಿಯೋ-ಟ್ಯಾಗ್‌ ಛಾಯಾಚಿತ್ರಗಳು (ಕನಿಷ್ಠ ಒಂದು ಹಂತ)",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "21",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಹಾಜರಾತಯಿ ಪಟ್ಟಿ ಚಲನಾ ಚೀಟಿ",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "22",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಕಾಮಗಾರಿ ಮುಕ್ತಾಯ ವರದಿ",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }], [
                {
                    text: "23",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "ಕಾಮಗಾರಿ ಸಾಮಾಜಿಕ ಪರಿಶೋಧನೆ ವರದಿಯ ಪ್ರತಿ",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    style: {
                        alignment: "left",
                        bold: true
                    }
                },
                {
                    text: "",
                    fontSize: 10,
                    colSpan: 1,
                    margin: [0, 0, 0, 0],
                    style: {
                        alignment: "center",
                        bold: true
                    }
                }]

        ]

        var docDefinition = {
            defaultStyle: {
                font: 'tunga',
            },
            background: [

            ],
            pageOrientation: 'portrait',
            pageMargins: [10, 20, 10, 10],
            content: [
                {
                    text: (reportData.LineDeptHeader || reportData.LineDeptNameRegional) + " ತಾ||" + reportData.blockNameRegional,
                    fontSize: 12,
                    style: {
                        bold: true,
                        alignment: "center"
                    }
                },{
                    text: "ಅನುಬಂಧ - || : ಕಾಮಗಾರಿ ಕಡತದ ಚೆಕ್ ಲಿಸ್ಟ",
                    fontSize: 10,
                    style: {
                        bold: true,
                        alignment: "center"
                    }

                },{
                    canvas: [
                        {
                            type: 'line',
                            x1: 0, y1: 10,
                            x2: 560, y2: 10,
                            lineWidth: 5,
                            color: '#b8860b'
                        }
                    ]
                },
                {
                    margin: [0, 10, 0, 0],
                    text: "ಕಾಮಗಾರಿ ಹೆಸರು                   : " + reportData.workName,
                    fontSize: 10,
                    style: {
                        bold: true,
                        alignment: "left"
                    }

                },
                {
                    text: "ಕಾಮಗಾರಿ ಸಂಕೇತ ಸಂಖ್ಯೆ       : " + reportData.workcode,
                    fontSize: 10,
                    style: {
                        bold: true,
                        alignment: "left"
                    }

                }, {
                    text: "ಕಾಮಗಾರಿ ಮಂಜೂರಾದ ವರ್ಷ : " + reportData.fincialYear,
                    fontSize: 10,
                    style: {
                        bold: true,
                        alignment: "left"
                    }

                }, {
                    table: {
                        widths: [30, '*', 80, 80],
                        headerRows: 1,
                        body: appArray,
                        dontBreakRows: true
                    },
                    margin: [0, 0, 0, 0]
                },
                {
                    margin: [0, 40, 0, 0],
                    text: [
                        {
                            text: reportData.LineDeptSign.split('#')[0] + '\n' + reportData.LineDeptSign.split('#')[1] || deptnamesign[reportData.LineDeptName] 
                        },
                        {
                            text:"ತಾ|| " + reportData.blockNameRegional,
                            style: {
                                bold: true
                            }
                        }
                    ],
                    fontSize: 10,
                    style: {
                        alignment: "right"
                    }
                }
            ]
        }

        let x = atob(getCookie('test').split('|')[1]);

        if (x === "No")
            docDefinition.background.push({
                image: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAkAAAAKDCAIAAAB9hCJ7AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAFiUAABYlAUlSJPAAAB7ISURBVHhe7d1NTmM71wbQbyC3WaNJK2OhkbEgMRTEUIoZvA2URgkJ6X4mpEJi7/OTEJ1436wltwofx2jbfo4cofq/fwEgIQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZBSXwH28bj+3z+/ftQeXvZj/fv6Z9X8NGjrt9Vm+/j0/nv/2JD3h+rB488aNzSTzfu+w7iXbf3gr+3z/mc9U82IahaqyXUIsKO22oxslcs3ye+nt+rBv23eWnfkFap5Y6oZEWA3JsCqth5afxdvkrFfatYIjrxCNW9MNSMC7MYEWNvWf6J3vUs3yfg05txUOPIK1bwx1YwIsBsTYFGLVv+Fm2T4juKrzVjujrxCNW9MNSMC7Mb6CrAhFy3QYJO8Pb7uf7jz8fy0jTdS8PJ12Sapt/1qXe+Z6UH+a5tENU8eUc1dU00ucNcBthMswfCm4qI5NIM/PDWzmrypcOQVqtkp1Tx9RIAtSoDFdyPtKrxkDs+b6pEybDvO1Ip35BWq2SnVPPMRrkmABUu5tKtskuaR3ctj+3ET4zjyCtXslGpWj6SuZjoCLNwkV7mmaBb36ukj/PeJmwpHXqGanVLN0/4CbFECLBo8WrVnz6HZe4dPb4caXfSOvEI1O6Wa5/Tnyu4+wIJXvNl7aXQOTf/vN8fgZn9sKEdeoZqdUs2qvwBb0j0H2Ot79BXx0KXBmXNoVvb+jmIn+AOUkZsKR16hmp1SzdPOAmxR9xVgM1pww/7lvDkM31HsBHMbXveOvEI1O6WasztzfQLsqK02QzukOGsOTed6751zU+HIK1SzU6pZdRZgSxJgX229fXz5vkaInDGHObcQZ9xUOPIK1eyUap72FGCLEmCf7e3h6BJ8wPw5zHuDC6Z3epVx4MgrVLNTqln1FGBLEmCHNvYfDhWz5xB8brimgwGPv0z+5sgrVLNTqln1TF3NdO4rwA6vUR/Pm+aWoLShi4JPc+cQ3D/Mb+HX1I68QjU7pZpVTwG2pDsNsE/RX5mMDDtzDsEdxTktuqlw5BWq2SnVrLoJsCXdcYCFww6vv3lzCD70vBbcVDjyCtXslGpW3VJXM527DrBw/Q1dVsyaw4/uKL5ae1PhyCtUs1OqWXUTYEu68wCLbxXabsWcOfzwjuKrzdnJjrzSVLMHqln1EWBLuvcAC7uFX9jOmEM01MQ8o0fqmwpHXqGanVLNqo8AW5IAG/jGuLnvnp5DdEcxuZqjF8NqiwabZLzFv2Y3VLPpM9ZUs26qyV8C7FMwftN5cg7RIMHbYi3aoqcf7cgrVLNTqll1GG+dVzMdAbYTf8F78o3x1Byipdy8KkYmH3TkFarZKdWsO4y2zquZjgDbi7/jPfqUiTlMv6wNCkY+eT105BWq2SnVbDqMtc6rmY4AO4jX4uGifHwOUwt91MQGc+QVqtkp1ax+Ot46r2Y6OQIMACoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZBSXwH28bj+3z+/ftQeXvZj/fv6Z9X8NGjrt9Vm+/j0/nv/2JD3h+rB488aNzSTzfu+w7iXbf3gr+3z/mc9U82IahaqyXUIsKO22oxslcs3ye+nt+rBv23eWnfkFap5Y6oZEWA3JsCqth5afxdvkrFfatYIjrxCNW9MNSMC7MYEWNvWf6J3vUs3yfg05txUOPIK1bwx1YwIsBsTYFGLVv+Fm2T4juKrzVjujrxCNW9MNSMC7Mb6CrAhFy3QYJO8Pb7uf7jz8fy0jTdS8PJ12Sapt/1qXe+Z6UH+a5tENU8eUc1dU00ucNcBthMswfCm4qI5NIM/PDWzmrypcOQVqtkp1Tx9RIAtSoDFdyPtKrxkDs+b6pEybDvO1Ip35BWq2SnVPPMRrkmABUu5tKtskuaR3ctj+3ET4zjyCtXslGpWj6SuZjoCLNwkV7mmaBb36ukj/PeJmwpHXqGanVLN0/4CbFECLBo8WrVnz6HZe4dPb4caXfSOvEI1O6Wa5/Tnyu4+wIJXvNl7aXQOTf/vN8fgZn9sKEdeoZqdUs2qvwBb0j0H2Ot79BXx0KXBmXNoVvb+jmIn+AOUkZsKR16hmp1SzdPOAmxR9xVgM1pww/7lvDkM31HsBHMbXveOvEI1O6WasztzfQLsqK02QzukOGsOTed6751zU+HIK1SzU6pZdRZgSxJgX229fXz5vkaInDGHObcQZ9xUOPIK1eyUap72FGCLEmCf7e3h6BJ8wPw5zHuDC6Z3epVx4MgrVLNTqln1FGBLEmCHNvYfDhWz5xB8brimgwGPv0z+5sgrVLNTqln1TF3NdO4rwA6vUR/Pm+aWoLShi4JPc+cQ3D/Mb+HX1I68QjU7pZpVTwG2pDsNsE/RX5mMDDtzDsEdxTktuqlw5BWq2SnVrLoJsCXdcYCFww6vv3lzCD70vBbcVDjyCtXslGpW3VJXM527DrBw/Q1dVsyaw4/uKL5ae1PhyCtUs1OqWXUTYEu68wCLbxXabsWcOfzwjuKrzdnJjrzSVLMHqln1EWBLuvcAC7uFX9jOmEM01MQ8o0fqmwpHXqGanVLNqo8AW5IAG/jGuLnvnp5DdEcxuZqjF8NqiwabZLzFv2Y3VLPpM9ZUs26qyV8C7FMwftN5cg7RIMHbYi3aoqcf7cgrVLNTqll1GG+dVzMdAbYTf8F78o3x1Byipdy8KkYmH3TkFarZKdWsO4y2zquZjgDbi7/jPfqUiTlMv6wNCkY+eT105BWq2SnVbDqMtc6rmY4AO4jX4uGifHwOUwt91MQGc+QVqtkp1ax+Ot46r2Y6OQIMACoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZBSXwH28bj+3z+/ftQeXvZj/fv6Z9X8NGjrt9Vm+/j0/nv/2JD3h+rB488aNzSTzfu+w7iXbf3gr+3z/mc9U82IahaqyXUIsKO22oxslcs3ye+nt+rBv23eWnfkFap5Y6oZEWA3JsCqth5afxdvkrFfatYIjrxCNW9MNSMC7MYEWNvWf6J3vUs3yfg05txUOPIK1bwx1YwIsBsTYFGLVv+Fm2T4juKrzVjujrxCNW9MNSMC7Mb6CrAhFy3QYJO8Pb7uf7jz8fy0jTdS8PJ12Sapt/1qXe+Z6UH+a5tENU8eUc1dU00ucNcBthMswfCm4qI5NIM/PDWzmrypcOQVqtkp1Tx9RIAtSoDFdyPtKrxkDs+b6pEybDvO1Ip35BWq2SnVPPMRrkmABUu5tKtskuaR3ctj+3ET4zjyCtXslGpWj6SuZjoCLNwkV7mmaBb36ukj/PeJmwpHXqGanVLN0/4CbFECLBo8WrVnz6HZe4dPb4caXfSOvEI1O6Wa5/Tnyu4+wIJXvNl7aXQOTf/vN8fgZn9sKEdeoZqdUs2qvwBb0j0H2Ot79BXx0KXBmXNoVvb+jmIn+AOUkZsKR16hmp1SzdPOAmxR9xVgM1pww/7lvDkM31HsBHMbXveOvEI1O6WasztzfQLsqK02QzukOGsOTed6751zU+HIK1SzU6pZdRZgSxJgX229fXz5vkaInDGHObcQZ9xUOPIK1eyUap72FGCLEmCf7e3h6BJ8wPw5zHuDC6Z3epVx4MgrVLNTqln1FGBLEmCHNvYfDhWz5xB8brimgwGPv0z+5sgrVLNTqln1TF3NdO4rwA6vUR/Pm+aWoLShi4JPc+cQ3D/Mb+HX1I68QjU7pZpVTwG2pDsNsE/RX5mMDDtzDsEdxTktuqlw5BWq2SnVrLoJsCXdcYCFww6vv3lzCD70vBbcVDjyCtXslGpW3VJXM527DrBw/Q1dVsyaw4/uKL5ae1PhyCtUs1OqWXUTYEu68wCLbxXabsWcOfzwjuKrzdnJjrzSVLMHqln1EWBLuvcAC7uFX9jOmEM01MQ8o0fqmwpHXqGanVLNqo8AW5IAG/jGuLnvnp5DdEcxuZqjF8NqiwabZLzFv2Y3VLPpM9ZUs26qyV8C7FMwftN5cg7RIMHbYi3aoqcf7cgrVLNTqll1GG+dVzMdAbYTf8F78o3x1Byipdy8KkYmH3TkFarZKdWsO4y2zquZjgDbi7/jPfqUiTlMv6wNCkY+eT105BWq2SnVbDqMtc6rmY4AO4jX4uGifHwOUwt91MQGc+QVqtkp1ax+Ot46r2Y6OQIMACoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZBSXwH28bj+3z+/ftQeXvZj/fv6Z9X8NGjrt9Vm+/j0/nv/2JD3h+rB488aNzSTzfu+w7iXbf3gr+3z/mc9U82IahaqyXUIsKO22oxslcs3ye+nt+rBv23eWnfkFap5Y6oZEWA3JsCqth5afxdvkrFfatYIjrxCNW9MNSMC7MYEWNvWf6J3vUs3yfg05txUOPIK1bwx1YwIsBsTYFGLVv+Fm2T4juKrzVjujrxCNW9MNSMC7Mb6CrAhFy3QYJO8Pb7uf7jz8fy0jTdS8PJ12Sapt/1qXe+Z6UH+a5tENU8eUc1dU00ucNcBthMswfCm4qI5NIM/PDWzmrypcOQVqtkp1Tx9RIAtSoDFdyPtKrxkDs+b6pEybDvO1Ip35BWq2SnVPPMRrkmABUu5tKtskuaR3ctj+3ET4zjyCtXslGpWj6SuZjoCLNwkV7mmaBb36ukj/PeJmwpHXqGanVLN0/4CbFECLBo8WrVnz6HZe4dPb4caXfSOvEI1O6Wa5/Tnyu4+wIJXvNl7aXQOTf/vN8fgZn9sKEdeoZqdUs2qvwBb0j0H2Ot79BXx0KXBmXNoVvb+jmIn+AOUkZsKR16hmp1SzdPOAmxR9xVgM1pww/7lvDkM31HsBHMbXveOvEI1O6WasztzfQLsqK02QzukOGsOTed6751zU+HIK1SzU6pZdRZgSxJgX229fXz5vkaInDGHObcQZ9xUOPIK1eyUap72FGCLEmCf7e3h6BJ8wPw5zHuDC6Z3epVx4MgrVLNTqln1FGBLEmCHNvYfDhWz5xB8brimgwGPv0z+5sgrVLNTqln1TF3NdO4rwA6vUR/Pm+aWoLShi4JPc+cQ3D/Mb+HX1I68QjU7pZpVTwG2pDsNsE/RX5mMDDtzDsEdxTktuqlw5BWq2SnVrLoJsCXdcYCFww6vv3lzCD70vBbcVDjyCtXslGpW3VJXM527DrBw/Q1dVsyaw4/uKL5ae1PhyCtUs1OqWXUTYEu68wCLbxXabsWcOfzwjuKrzdnJjrzSVLMHqln1EWBLuvcAC7uFX9jOmEM01MQ8o0fqmwpHXqGanVLNqo8AW5IAG/jGuLnvnp5DdEcxuZqjF8NqiwabZLzFv2Y3VLPpM9ZUs26qyV8C7FMwftN5cg7RIMHbYi3aoqcf7cgrVLNTqll1GG+dVzMdAbYTf8F78o3x1Byipdy8KkYmH3TkFarZKdWsO4y2zquZjgDbi7/jPfqUiTlMv6wNCkY+eT105BWq2SnVbDqMtc6rmY4AO4jX4uGifHwOUwt91MQGc+QVqtkp1ax+Ot46r2Y6OQIMACoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZBSXwH28bj+3z+/ftQeXvZj/fv6Z9X8NGjrt9Vm+/j0/nv/2JD3h+rB488aNzSTzfu+w7iXbf3gr+3z/mc9U82IahaqyXUIsKO22oxslcs3ye+nt+rBv23eWnfkFap5Y6oZEWA3JsCqth5afxdvkrFfatYIjrxCNW9MNSMC7MYEWNvWf6J3vUs3yfg05txUOPIK1bwx1YwIsBsTYFGLVv+Fm2T4juKrzVjujrxCNW9MNSMC7Mb6CrAhFy3QYJO8Pb7uf7jz8fy0jTdS8PJ12Sapt/1qXe+Z6UH+a5tENU8eUc1dU00ucNcBthMswfCm4qI5NIM/PDWzmrypcOQVqtkp1Tx9RIAtSoDFdyPtKrxkDs+b6pEybDvO1Ip35BWq2SnVPPMRrkmABUu5tKtskuaR3ctj+3ET4zjyCtXslGpWj6SuZjoCLNwkV7mmaBb36ukj/PeJmwpHXqGanVLN0/4CbFECLBo8WrVnz6HZe4dPb4caXfSOvEI1O6Wa5/Tnyu4+wIJXvNl7aXQOTf/vN8fgZn9sKEdeoZqdUs2qvwBb0j0H2Ot79BXx0KXBmXNoVvb+jmIn+AOUkZsKR16hmp1SzdPOAmxR9xVgM1pww/7lvDkM31HsBHMbXveOvEI1O6WasztzfQLsqK02QzukOGsOTed6751zU+HIK1SzU6pZdRZgSxJgX229fXz5vkaInDGHObcQZ9xUOPIK1eyUap72FGCLEmCf7e3h6BJ8wPw5zHuDC6Z3epVx4MgrVLNTqln1FGBLEmCHNvYfDhWz5xB8brimgwGPv0z+5sgrVLNTqln1TF3NdO4rwA6vUR/Pm+aWoLShi4JPc+cQ3D/Mb+HX1I68QjU7pZpVTwG2pDsNsE/RX5mMDDtzDsEdxTktuqlw5BWq2SnVrLoJsCXdcYCFww6vv3lzCD70vBbcVDjyCtXslGpW3VJXM527DrBw/Q1dVsyaw4/uKL5ae1PhyCtUs1OqWXUTYEu68wCLbxXabsWcOfzwjuKrzdnJjrzSVLMHqln1EWBLuvcAC7uFX9jOmEM01MQ8o0fqmwpHXqGanVLNqo8AW5IAG/jGuLnvnp5DdEcxuZqjF8NqiwabZLzFv2Y3VLPpM9ZUs26qyV8C7FMwftN5cg7RIMHbYi3aoqcf7cgrVLNTqll1GG+dVzMdAbYTf8F78o3x1Byipdy8KkYmH3TkFarZKdWsO4y2zquZjgDbi7/jPfqUiTlMv6wNCkY+eT105BWq2SnVbDqMtc6rmY4AO4jX4uGifHwOUwt91MQGc+QVqtkp1ax+Ot46r2Y6OQIMACoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZBSXwH28bj+3z+/ftQeXvZj/fv6Z9X8NGjrt9Vm+/j0/nv/2JD3h+rB488aNzSTzfu+w7iXbf3gr+3z/mc9U82IahaqyXUIsKO22oxslcs3ye+nt+rBv23eWnfkFap5Y6oZEWA3JsCqth5afxdvkrFfatYIjrxCNW9MNSMC7MYEWNvWf6J3vUs3yfg05txUOPIK1bwx1YwIsBsTYFGLVv+Fm2T4juKrzVjujrxCNW9MNSMC7Mb6CrAhFy3QYJO8Pb7uf7jz8fy0jTdS8PJ12Sapt/1qXe+Z6UH+a5tENU8eUc1dU00ucNcBthMswfCm4qI5NIM/PDWzmrypcOQVqtkp1Tx9RIAtSoDFdyPtKrxkDs+b6pEybDvO1Ip35BWq2SnVPPMRrkmABUu5tKtskuaR3ctj+3ET4zjyCtXslGpWj6SuZjoCLNwkV7mmaBb36ukj/PeJmwpHXqGanVLN0/4CbFECLBo8WrVnz6HZe4dPb4caXfSOvEI1O6Wa5/Tnyu4+wIJXvNl7aXQOTf/vN8fgZn9sKEdeoZqdUs2qvwBb0j0H2Ot79BXx0KXBmXNoVvb+jmIn+AOUkZsKR16hmp1SzdPOAmxR9xVgM1pww/7lvDkM31HsBHMbXveOvEI1O6WasztzfQLsqK02QzukOGsOTed6751zU+HIK1SzU6pZdRZgSxJgX229fXz5vkaInDGHObcQZ9xUOPIK1eyUap72FGCLEmCf7e3h6BJ8wPw5zHuDC6Z3epVx4MgrVLNTqln1FGBLEmCHNvYfDhWz5xB8brimgwGPv0z+5sgrVLNTqln1TF3NdO4rwA6vUR/Pm+aWoLShi4JPc+cQ3D/Mb+HX1I68QjU7pZpVTwG2pDsNsE/RX5mMDDtzDsEdxTktuqlw5BWq2SnVrLoJsCXdcYCFww6vv3lzCD70vBbcVDjyCtXslGpW3VJXM527DrBw/Q1dVsyaw4/uKL5ae1PhyCtUs1OqWXUTYEu68wCLbxXabsWcOfzwjuKrzdnJjrzSVLMHqln1EWBLuvcAC7uFX9jOmEM01MQ8o0fqmwpHXqGanVLNqo8AW5IAG/jGuLnvnp5DdEcxuZqjF8NqiwabZLzFv2Y3VLPpM9ZUs26qyV8C7FMwftN5cg7RIMHbYi3aoqcf7cgrVLNTqll1GG+dVzMdAbYTf8F78o3x1Byipdy8KkYmH3TkFarZKdWsO4y2zquZjgDbi7/jPfqUiTlMv6wNCkY+eT105BWq2SnVbDqMtc6rmY4AO4jX4uGifHwOUwt91MQGc+QVqtkp1ax+Ot46r2Y6OQIMACoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICUBBgAKQkwAFISYACkJMAASEmAAZCSAAMgJQEGQEoCDICE/v33/wFcLQuO72hbcQAAAABJRU5ErkJggg==',
                width: 700
            })

        generateNewPdf(docDefinition, reportData.workcode + "_checklist");

        $('#imgDown').hide();
        $('#downSuc').show();
        setTimeout(function () { $('#downSuc').hide(); $('#progressBar').hide(); }, 3000);
    } catch (e) {
        console.log(e.message);
    }

}