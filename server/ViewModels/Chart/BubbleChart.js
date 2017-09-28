import React from 'react';
import PropTypes from 'prop-types';

import moment from 'moment';

import { min, max } from 'd3-array';
import { axisBottom, axisLeft } from 'd3-axis';
import { scaleLinear, scaleTime } from 'd3-scale';
import { select } from 'd3-selection';
import { timeFormat } from 'd3-time-format';

import * as Constants from '../Constants';

class BubbleChart extends React.Component {
  constructor(props) {
    super(props);
    this.fullWidth = 400;
    this.fullHeight = 150;
    this.margins = {top: 10, right: 10, bottom: 15, left: 20};
    this.radius = 2.5;
  }

  componentDidMount() {
    this.createBubbleChart();
  }

  componentDidUpdate() {
    this.createBubbleChart();
  }

  createBubbleChart() {
    const node = this.node;
    const width = this.fullWidth - this.margins.left - this.margins.right;
    const height = this.fullHeight - this.margins.top - this.margins.bottom;

    const startDate = moment()
      .startOf('day')
      .subtract(this.props.chart.time, this.props.chart.timeUnit)
      .toDate();
    const endDate = moment()
      .startOf('day')
      .toDate();

    const xScale = scaleTime()
      .domain([startDate, endDate])
      .range([this.margins.left, this.margins.left + width]);
    
    const values = this.props.chartPoints.map(point => point[this.props.chart.field]);
    const minValue = min(values);
    const maxValue = max(values);
    const yScale = scaleLinear()
      .domain([minValue, maxValue])
      .range([this.margins.top + height, this.margins.top])
      .nice(Math.ceil(maxValue) - Math.floor(minValue));

    var xAxis = axisBottom(xScale)
      .tickSizeInner(-height)
      .tickSizeOuter(0)
      .tickFormat(timeFormat('%b'))
      .tickPadding(5);

    var yAxis = axisLeft(yScale)
      .tickSizeInner(-width)
      .tickSizeOuter(0)
      .tickPadding(5);
      
    select(node)
      .append('g')
      .attr('transform', `translate(${this.margins.left}, 0)`)
      .call(yAxis);

    select(node)
      .append('g')
      .attr('transform', `translate(0, ${this.margins.top + height})`)
      .call(xAxis);

    var group = select(node)
      .selectAll('g.bubble')
      .data(this.props.chartPoints)
      .enter()
      .append('g')
      .attr('transform', (point) => {
        var x = xScale(moment(point.date, Constants.dateFormat).toDate());
        var y = yScale(point[this.props.chart.field]);
        return `translate(${x}, ${y})`;
      });
    
    group
      .append('circle')
      .attr('r', `${this.radius}`)
      .style('fill', this.props.chart.color);
  }

  render() {
    return (
      <div>
        <style scoped dangerouslySetInnerHTML={{__html: `
          path, line {
            stroke-width: 1px;
            shape-rendering: crispEdges;
            vector-effect: non-scaling-stroke;
          }
          path.domain {
            stroke: #9E9E9E;
          }
          .tick > line {
            stroke: #E0E0E0;
          }
          text {
            font-family: Roboto;
            font-size: 8px;
          }
        `}} />
        <svg
          ref={node => this.node = node}
          style={{display: 'block'}}
          width="100%"
          viewBox={`0 0 ${this.fullWidth} ${this.fullHeight}`}
          preserveAspectRatio="xMidYMin meet">
        </svg>
      </div>
    );
  }
}

BubbleChart.propTypes = {
  chart: PropTypes.object,
  chartPoints: PropTypes.array
};

export default BubbleChart;