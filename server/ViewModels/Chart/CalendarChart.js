import React from 'react';
import PropTypes from 'prop-types';

import { select } from 'd3-selection';
import { timeDay } from 'd3-time';
import moment from 'moment';

import * as Constants from '../Constants';

class CalendarChart extends React.Component {
  constructor(props) {
    super(props);
    this.fullWidth = 800;
    this.fullHeight = 300;
    this.daySide = 10;
    this.dayPadding = 2;
    this.backgroundColor = "#E9E9E9";
    this.emptyDayColor = "#E9E9E9";
    this.monthBoundaryColor = "#FFFFFF";
  }
  
  componentDidMount() {
    this.createCalendarChart();
  }

  componentDidUpdate() {
    this.createCalendarChart();
  }

  createCalendarChart() {
    const node = this.node;

    const dayMap = this.props.chartPoints.reduce((dayMap, point) => {
      var date = moment(point.date, Constants.dateFormat).toDate();
      var currentValue = dayMap[date] ? dayMap[date] : 0;
      var increment = this.props.chart.field ? point[this.props.chart.field] : 1;
      return Object.assign(dayMap, {[date]: currentValue + increment});
    }, {});

    let getColor = (value) => {
      const colors = this.props.chart.colors;
      if (!value) {
        return this.emptyDayColor;
      }
      if (colors[value]) {
        return colors[value - 1];
      }
      return colors[colors.length - 1];
    };

    let startDate = moment()
      .startOf('day')
      .subtract(this.props.chart.time, this.props.chart.timeUnit);
    while (startDate.day() != 1) {
      startDate = startDate.subtract(1, 'days');
    }
    startDate = startDate.toDate();

    const endDateMoment = moment()
      .startOf('day');
    
    const endDate = endDateMoment.toDate();
    const rangeEndDate = endDateMoment
      .add(1, 'days') // timeDay.range() below is not inclusive
      .toDate();
    const dateRange = timeDay.range(startDate, rangeEndDate);

    let monthBoundariesEndDate = moment()
      .startOf('day')
      .add(1, 'days') // same
      .add(1, 'week');
    while (monthBoundariesEndDate.day() != 0) {
      monthBoundariesEndDate = monthBoundariesEndDate.add(1, 'days');
    }
    monthBoundariesEndDate = monthBoundariesEndDate.toDate();
    const monthVerticalBoundariesDateRange = timeDay
      .range(startDate, monthBoundariesEndDate)
      .filter(date => date.getDate() <= 7);

    const monthHorizontalBoundariesDateRange = monthVerticalBoundariesDateRange
      .filter(date => date.getDate() == 1)
      .filter(date => moment(date).day() != 1);

    let getColumn = (date) => 
      moment(date).diff(startDate, 'weeks');

    let getRow = (date) => {
      let row = 0;
      let dateIt = moment(date);
      while (dateIt.day() != 1) {
        row++;
        dateIt = dateIt.subtract(1, 'days');
      }
      return row;
    };

    select(node)
      .append('rect')
      .attr('width', getColumn(endDate) * (this.daySide + this.dayPadding) + this.daySide)
      .attr('height', 7 * (this.daySide + this.dayPadding) - this.dayPadding)
      .attr('fill', this.backgroundColor)
      .attr('x', 0)
      .attr('y', 0);

    select(node)
      .selectAll('g.day')
      .data(dateRange)
      .enter()
      .append('rect')
      .attr('width', this.daySide)
      .attr('height', this.daySide)
      .attr('fill', (date) => getColor(dayMap[date]))
      .attr('x', (date) => getColumn(date) * (this.daySide + this.dayPadding))
      .attr('y', (date) => getRow(date) * (this.daySide + this.dayPadding));
    
    select(node)
      .selectAll('g.vertical-month')
      .data(monthVerticalBoundariesDateRange)
      .enter()
      .append('rect')
      .attr('width', this.dayPadding)
      .attr('height', this.daySide + this.dayPadding * 2)
      .attr('fill', this.monthBoundaryColor)
      .attr('x', (date) => getColumn(date) * (this.daySide + this.dayPadding) - this.dayPadding)
      .attr('y', (date) => getRow(date) * (this.daySide + this.dayPadding) - this.dayPadding);
    
    select(node)
      .selectAll('g.horizontal-month')
      .data(monthHorizontalBoundariesDateRange)
      .enter()
      .append('rect')
      .attr('width', this.daySide + this.dayPadding * 2)
      .attr('height', this.dayPadding)
      .attr('fill', this.monthBoundaryColor)
      .attr('x', (date) => getColumn(date) * (this.daySide + this.dayPadding) - this.dayPadding)
      .attr('y', (date) => getRow(date) * (this.daySide + this.dayPadding) - this.dayPadding);
  }

  render() {
    return (
      <div>
        <style scoped dangerouslySetInnerHTML={{__html: `
          path, line, rect {
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

CalendarChart.propTypes = {
  chart: PropTypes.object,
  chartPoints: PropTypes.array
};

export default CalendarChart;