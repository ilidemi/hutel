import React from 'react';
import PropTypes from 'prop-types';

import $ from 'jquery';
import moment from 'moment';

import Divider from 'material-ui/Divider';
import LinearProgress from 'material-ui/LinearProgress';

import * as Constants from './Constants';

class Calendar extends React.Component {
  constructor(props) {
    super(props);
    this.lookbackDays = 7 * 4;
    this.state = {
      loading: true,
      request: null,
      pointCounts: null
    };
  }

  componentDidMount() {
    var today = moment();
    var startDate = today.clone()
      .subtract(today.day() - 1 + this.lookbackDays, 'days')
      .format(Constants.dateFormat);
    $.ajax({
      url: `/api/points?tagId=${this.props.tagId}&startDate=${startDate}`,
      dataType: "json",
      method: "GET",
      success: (data) => {
        this.setState({
          loading: false,
          request: null,
          pointCounts: data.reduce(
            (total, point) => {
              total[point.date] = (total[point.date] | 0) + 1;
              return total;
            },
            {}
          )
        });
      },
      error: (xhr, status, err) => {
        console.error(err);
        this.setState({
          loading: false,
          request: null
        });
      }
    });
  }

  componentWillUnmount() {
    if (this.state.request !== null) {
      this.state.request.cancel();
    }
  }

  renderCell(content, key, hasPoint, isToday) {
    const containerStyle = isToday ? { background: '#f5f5f5' } : {};
    const fillerStyle = {
      content: '',
      display: 'inline-block',
      width: 0,
      height: 0,
      paddingBottom: '100%'
    };
    const pointStyle = {
      borderRadius: '50%',
      background: '#ff6f00',
      margin: '20%'
    };
    const defaultCellStyle = {
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center'
    };
    const pointCellStyle = Object.assign({}, defaultCellStyle, pointStyle);
    const cellStyle = hasPoint ? pointCellStyle : defaultCellStyle;

    return (
      <div style={containerStyle} key={key}>
        <div style={cellStyle}>
          <div style={fillerStyle} />
          <div style={defaultCellStyle}>{content}</div>
        </div>
      </div>
    );
  }

  render() {
    if (this.state.loading) {
      return <LinearProgress />;
    }

    var today = moment();
    var totalDays = today.day() + this.lookbackDays;
    var items = [];
    var todayKey = today.format(Constants.dateFormat);
    var daysOfWeek = ['M', 'T', 'W', 'T', 'F', 'S', 'S'];
    for (var i = 0; i < daysOfWeek.length; i++) {
      items.push(
        this.renderCell(<b>{daysOfWeek[i]}</b>, i, false, false)
      );
    }
    for (i = 0; i < totalDays; i++) {
      var day = today.clone().subtract(totalDays - i - 1, 'days');
      var dayKey = day.format(Constants.dateFormat);
      var hasPoint = dayKey in this.state.pointCounts;
      var isToday = dayKey === todayKey;
      items.push(
        this.renderCell(day.date(), dayKey, hasPoint, isToday)
      );
    }
    const gridStyle = {
      display: 'grid',
      gridTemplateColumns: 'repeat(7, 1fr)',
      width: '100%',
      maxWidth: '500px',
      margin: 'auto',
      padding: '30px'
    };
    return (
      <div>
        <Divider />
        <div style={gridStyle}>{items}</div>
        <Divider />
      </div>
    );
  }
}

Calendar.propTypes = {
  tagId: PropTypes.string,
  theme: PropTypes.object
};

export default Calendar;