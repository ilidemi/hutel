var app = new Vue({
  el: '#app',
  data: {
    message: 'Hello Vue!',
    tags: [],
    selectedTag: null,
    tagInput: null
  },
  created: function() {
    var resource = this.$resource('api/tags');
    resource.get().then(response => {
      this.tags = response.body;
    }, response => {
      this.message = "fail";
    });
  },
  methods: {
    selectTag: function(tag) {
      this.tagInput = {};
      for (var field of tag.fields) {
        this.$set(this.tagInput, field.name, "");
      }
      this.$set(this.tagInput, "date", "");
      this.$set(this.tagInput, "tagId", tag.id);
      this.selectedTag = tag;
    },
    resetTag: function() {
      this.selectedTag = null;
      this.tagInput = null;
    },
    todayDate: function() {
      var date = new Date();
      var result = moment().format('YYYY-MM-DD');
      this.$set(this.tagInput, "date", result);
    },
    yesterdayDate: function() {
      var date = new Date();
      var result = moment().subtract(1, 'days').format('YYYY-MM-DD');
      this.$set(this.tagInput, "date", result);
    },
    submit: function() {
      var resource = this.$resource('api/points');
      resource.save(this.tagInput).then(response => {
        this.message = response.body;
        this.resetTag();
      }, response => {
        this.message = response.body;
        this.resetTag();
      })
      console.log(JSON.stringify(this.tagInput));
    }
  }
});