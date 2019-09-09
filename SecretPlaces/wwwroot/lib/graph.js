// pie chart for popular reviews
function createPopularReviewsGraph(data, id) {
    var width = 400,
        height = 400,
        radius = Math.min(width, height) / 2;

    var color = d3.scaleOrdinal()
        .domain(data)
        .range(["#a05d56", "#6b486b", "#ff8c00", "#98abc5", "#8a89a6", "#d0743c", "#7b6888"]);

    var arc = d3.arc()
        .outerRadius(radius - 10)
        .innerRadius(0);

    var pie = d3.pie()
        .sort(null)
        .value(function (d) { return d.totalReviews; });

    var svg = d3.select("#" + id).append("svg")
        .attr("width", width)
        .attr("height", height)
        .append("g")
        .attr("transform", "translate(" + width / 2 + "," + height / 2 + ")");

    data.forEach(function (d) {
        d.PlaceName = d.PlaceName;
        d.totalReviews = +d.totalReviews;
    });

    var g = svg.selectAll(".arc")
        .data(pie(data))
        .enter().append("g")
        .attr("class", "arc");

    g.append("path")
        .attr("d", arc)
        .style("fill", function (d) { return color(d.data.totalReviews * Math.floor(Math.random() * 100) % 15); });

    g.append("text")
        .attr("transform", function (d) { return "translate(" + arc.centroid(d) + ")"; })
        .attr("dy", ".35em")
        .style("text-anchor", "middle")
        .text(function (d) { return d.data.PlaceName; });

}

// bar chart
function createBarChartGraph(data, id, text) {
    var svg = d3.select("#" + id),
        margin = {
            top: 20,
            right: 20,
            bottom: 30,
            left: 50
        },
        width = +svg.attr("width") - margin.left - margin.right,
        height = +svg.attr("height") - margin.top - margin.bottom,
        g = svg.append("g").attr("transform", "translate(" + margin.left + "," + margin.top + ")");

    // set the ranges
    var x = d3.scaleBand()
        .rangeRound([0, width])
        .padding(0.1);

    var y = d3.scaleLinear()
        .rangeRound([height, 0]);

    // load the data
    data.forEach(function (d) {
        d.name = d.name;
        d.count = +d.count;
    });

    // scale the range of the data
    x.domain(data.map(function (d) { return d.name; }));
    y.domain([0, d3.max(data, function (d) { return d.count; })]);

    // add axis
    g.append("g")
        .attr("transform", "translate(0," + height + ")")
        .call(d3.axisBottom(x))

    g.append("g")
        .call(d3.axisLeft(y))
        .append("text")
        .attr("fill", "#000")
        .attr("transform", "rotate(-90)")
        .attr("y", 6)
        .attr("dy", "0.71em")
        .attr("text-anchor", "end")
        .text(text);

    // add bar chart
    g.selectAll(".bar")
        .data(data)
        .enter().append("rect")
        .attr("class", "bar")
        .attr("x", function (d) {
            return x(d.name);
        })
        .attr("y", function (d) {
            return y(Number(d.count));
        })
        .attr("width", x.bandwidth())
        .attr("height", function (d) {
            return height - y(Number(d.count));
        }); 
}