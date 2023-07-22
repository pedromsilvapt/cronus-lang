let fib :: Int n -> Int {
	let n1 = n - 1;
	let n2 = n - 2;

	if n <= 2 then 1 else ((fib n1) + (fib n2))
}

let fib2 :: Int n -> Int {
	let n1 = n - 1;
	let n2 = n - 2;

	if n <= 2 then 1 else {
		let f1 = fib n1;
		let f2 = fib n2;
		
		f1 + f2
	}
}

let main :: None -> Int = fib2 5;