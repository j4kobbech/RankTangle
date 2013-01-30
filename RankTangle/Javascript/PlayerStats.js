jQuery(window).load(function () {
    // Render the Players rating chart
    var container = $("#player-rating-chart")[0];
    getChartData(container);

    // Add a tag on the players that match the current user
    var currentUserId = $("#current-user-id").val();
    if (!!currentUserId) {
        $("." + currentUserId).append("<sup>(me)</sup>");
    }
});

function getChartData(container) {
    var subjectId = $(container).attr("data-subject-id");
    var subjectName = $(container).attr("data-subject-name");
    var chartData = { DataPoints: [] };

    $.ajax({
        type: "get",
        cache: false,
        url: "/Stats/GetPlayerRatingData",
        data: { playerId: subjectId },
        success: function (data) {
            $.each(data.DataPoints, function (key, value) {
                chartData.DataPoints.push([
                    new Date(Date.UTC(
                        value.TimeSet[0], // year
                        parseInt(value.TimeSet[1]) - 1, // month (0-based)
                        value.TimeSet[2], // day
                        value.TimeSet[3], // hour
                        value.TimeSet[4],  // minute
                        value.TimeSet[5]  // second
                    )).toDateString(),
                    parseFloat(value.Rating.toPrecision(6))
                ]);
            });

            chartData.MinimumValue = data.MinimumValue - 50;
            chartData.MaximumValue = data.MaximumValue + 50;
            chartData.PlayerName = subjectName;
            renderChart(chartData);
        }
    });
}

function renderChart(chartData) {
    var chart = new Highcharts.Chart({
        chart: {
            renderTo: 'player-rating-chart',
            zoomType: 'x',
            spacingRight: 10
        },
        title: {
            text: chartData.PlayerName + 's rating over time'
        },
        subtitle: {
            text: document.ontouchstart === undefined
                    ? 'Click and drag in the plot area to zoom in'
                    : 'Drag your finger over the plot to zoom in'
        },
        xAxis: {
            title: {
                text: null
            }
        },
        yAxis: {
            title: {
                text: null
            },
            min: chartData.MinimumValue,
            max: chartData.MaximumValue,
            startOnTick: false,
            showFirstLabel: false
        },
        tooltip: {
            shared: true
        },
        legend: {
            enabled: false
        },
        plotOptions: {
            area: {
                fillColor: {
                    linearGradient: { x1: 0, y1: 0, x2: 0, y2: 1 },
                    stops: [
                        [0, Highcharts.getOptions().colors[0]],
                        [1, 'rgba(2,0,0,0)']
                    ]
                },
                lineWidth: 1,
                marker: {
                    enabled: false,
                    states: {
                        hover: {
                            enabled: true,
                            radius: 5
                        }
                    }
                },
                shadow: false,
                states: {
                    hover: {
                        lineWidth: 1
                    }
                }
            }
        },
        series: [{
            type: 'area',
            name: 'Rating',
            data: chartData.DataPoints
        }]
    });
}