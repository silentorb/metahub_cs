window.forest = window.forest || {}
forest.Ground = function() {}
forest.Ground.prototype = {}
forest.Ground.prototype._acorn_count = 0
Object.defineProperty(forest.Ground.prototype, 'acorn_count', {
	get: function() {
		return this._acorn_count
	},
	set: function(value) {
		this._acorn_count = value
	}
})
forest.Tree = function() {}
forest.Tree.prototype = {}
forest.Tree.prototype.acorn_count = 0