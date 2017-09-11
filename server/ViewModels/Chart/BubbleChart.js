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
  }

  componentDidMount() {
    this.createBarChart();
  }

  componentDidUpdate() {
    this.createBarChart();
  }

  createBarChart() {
    const node = this.node;
    const margin = {top: 20, right: 20, bottom: 30, left: 40, innerBottom: 5, innerLeft: 5};
    const width = 800 - margin.left - margin.right;
    const height = 300 - margin.top - margin.bottom;

    const startDate = moment()
      .startOf('day')
      .subtract(this.props.chart.time, this.props.chart.timeUnit)
      .toDate();
    const endDate = moment()
      .startOf('day')
      .toDate();

    const xScale = scaleTime()
      .domain([startDate, endDate])
      .range([margin.left, margin.left + width]);
    
    const values = this.props.chartPoints.map(point => point[this.props.chart.field]);
    const yScale = scaleLinear()
      .domain([min(values), max(values)])
      .range([margin.top + height, margin.top])
      .nice();

    var xAxis = axisBottom(xScale)
      .tickSize(5)
      .tickFormat(timeFormat('%b'))
      .tickPadding(5);

    var yAxis = axisLeft(yScale)
      .tickSize(5);

    select(node)
      .append('g')
      .attr('transform', `translate(0, ${margin.top + height})`)
      .call(xAxis);
    
    select(node)
      .append('g')
      .attr('transform', `translate(${margin.left}, 0)`)
      .call(yAxis);

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
      .attr('r', '5')
      .style('fill', this.props.chart.color);
  }

  render() {
    return (
      <svg
        ref={node => this.node = node}
        style={{display: 'block'}}
        width="100%"
        viewBox="0 0 800 300"
        preserveAspectRatio="xMidYMin meet">
      </svg>
    );
  }
}

BubbleChart.propTypes = {
  chart: PropTypes.object,
  chartPoints: PropTypes.array
};

export default BubbleChart;